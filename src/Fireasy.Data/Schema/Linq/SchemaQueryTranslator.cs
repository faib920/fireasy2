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
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Schema.Linq
{
    internal sealed class SchemaQueryTranslator : Common.Linq.Expressions.ExpressionVisitor
    {
        private Type _metadataType;
        private string _memberName;
        private List<MemberInfo> _members = null;
        private readonly RestrictionDictionary _restrDict = new RestrictionDictionary();

        /// <summary>
        /// 对表达式进行解析，并返回限制值字典。
        /// </summary>
        /// <param name="expression">查询表达式。</param>
        /// <param name="dicRestrMbrs"></param>
        /// <returns></returns>
        public static RestrictionDictionary GetRestrictions<T>(Expression expression, Dictionary<Type, List<MemberInfo>> dicRestrMbrs)
        {
            if (expression == null)
            {
                return RestrictionDictionary.Empty;
            }

            var translator = new SchemaQueryTranslator { _metadataType = typeof(T) };

            if (!dicRestrMbrs.TryGetValue(typeof(T), out List<MemberInfo> properties))
            {
                throw new SchemaQueryTranslateException(typeof(T));
            }

            translator._members = properties;
            expression = PartialEvaluator.Eval(expression);
            translator.Visit(expression);
            return translator._restrDict;
        }

        /// <summary>
        /// 访问表达式树。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override Expression Visit(Expression expression)
        {
            return expression.NodeType switch
            {
                ExpressionType.MemberAccess => VisitMember((MemberExpression)expression),
                ExpressionType.Equal => VisitBinary((BinaryExpression)expression),
                ExpressionType.Constant => VisitConstant((ConstantExpression)expression),
                _ => base.Visit(expression),
            };
        }

        /// <summary>
        /// 访问二元运算表达式。
        /// </summary>
        /// <param name="binaryExp"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression binaryExp)
        {
            _memberName = string.Empty;

            if (binaryExp.Right is MemberExpression rmbr && rmbr.Member.DeclaringType == _metadataType)
            {
                Visit(binaryExp.Right);
                Visit(binaryExp.Left);
            }
            else if (binaryExp.Left is MemberExpression lmbr && lmbr.Member.DeclaringType == _metadataType)
            {
                Visit(binaryExp.Left);
                Visit(binaryExp.Right);
            }
            else
            {
                Visit(binaryExp.Left);
                Visit(binaryExp.Right);
            }

            return binaryExp;
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            //如果属性是架构元数据类的成员
            if (memberExp.Member.DeclaringType == _metadataType)
            {
                if (!_members.Contains(memberExp.Member))
                {
                    throw new SchemaQueryTranslateException(memberExp.Member, _members);
                }

                _memberName = memberExp.Member.Name;
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
            if (!string.IsNullOrEmpty(_memberName))
            {
                _restrDict[_memberName] = constExp.Value;
            }

            return constExp;
        }
    }
}
