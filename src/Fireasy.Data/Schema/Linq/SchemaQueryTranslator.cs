// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Schema.Linq
{
    internal sealed class SchemaQueryTranslator : Common.Linq.Expressions.ExpressionVisitor
    {
        private readonly Dictionary<int, string> dicRestr;
        private int currIndex = -1;
        private int maxIndex;
        private readonly Type metadataType;
        private readonly Dictionary<string, int> indexes;

        public SchemaQueryTranslator(Dictionary<string, int> indexes, Type metadataType)
        {
            this.indexes = indexes;
            this.metadataType = metadataType;
            dicRestr = new Dictionary<int, string>();
            foreach (var index in indexes)
            {
                //使用索引作为键值
                dicRestr.Add(index.Value, null);
            }
        }

        /// <summary>
        /// 对表达式进行解析，并返回限制数组。
        /// </summary>
        /// <param name="indexes">数据提供者类别。</param>
        /// <param name="metadataType">架构元数组类型。</param>
        /// <param name="expression">查询表达式。</param>
        /// <returns></returns>
        public static string[] GetRestriction(Dictionary<string, int> indexes, Type metadataType, Expression expression)
        {
            var translator = new SchemaQueryTranslator(indexes, metadataType);
            expression = PartialEvaluator.Eval(expression);
            return translator.GetRestrictionValues(expression);
        }

        private string[] GetRestrictionValues(Expression expression)
        {
            if (expression != null)
            {
                Visit(expression);
            }
            return TrimEmptyArray();
        }

        /// <summary>
        /// 访问表达式树。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override Expression Visit(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)expression);
                case ExpressionType.Equal:
                    return VisitBinary((BinaryExpression)expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);
            }
            return base.Visit(expression);
        }

        /// <summary>
        /// 访问二元运算表达式。
        /// </summary>
        /// <param name="binaryExp"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression binaryExp)
        {
            //属性在运算符的右边
            var memberExp = binaryExp.Right as MemberExpression;
            if (memberExp != null &&
                memberExp.Member.DeclaringType == metadataType)
            {
                Visit(binaryExp.Right);
                Visit(binaryExp.Left);
            }
            else
            {
                Visit(binaryExp.Left);
                Visit(binaryExp.Right);
            }
            //复位
            currIndex = -1;
            return binaryExp;
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            //如果属性是架构元数据类的成员
            if (memberExp.Member.DeclaringType == metadataType)
            {
                var mbr = indexes.FirstOrDefault(s => s.Key.Equals(memberExp.Member.Name));
                //记录下当前的索引，以及目前的最大索引
                currIndex = mbr.Value;
                maxIndex = Math.Max(maxIndex, currIndex + 1);
                return memberExp;
            }

            //值或引用
            var exp = (Expression)memberExp;
            if (memberExp.Type.IsValueType)
            {
                exp = Expression.Convert(memberExp, typeof(object));
            }
            var lambda = Expression.Lambda<Func<object>>(exp);
            var fn = lambda.Compile();
            //转换为常量表达式
            return Visit(Expression.Constant(fn(), memberExp.Type));
        }

        protected override Expression VisitConstant(ConstantExpression constExp)
        {
            if (currIndex == -1 || constExp.Value == null)
            {
                return constExp;
            }

            //没有复位的情况下，记录值
            dicRestr[currIndex] = constExp.Value.ToString();
            return constExp;
        }

        /// <summary>
        /// 删除空的数据元素
        /// </summary>
        /// <returns></returns>
        private string[] TrimEmptyArray()
        {
            //最大范围
            var array = new string[maxIndex];
            for (var i = 0; i < maxIndex; i++)
            {
                if (dicRestr.ContainsKey(i))
                {
                    array[i] = dicRestr[i];
                }
            }
            return array;
        }
    }
}
