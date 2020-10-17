﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 如果 <see cref="JoinExpression"/> 表达式中的一边具有 Group 子表，则需要将连接条件中的 Key 表达式替换为相应的 <see cref="ColumnExpression"/> 对象。
    /// </summary>
    public class GroupKeyReplacer : DbExpressionVisitor
    {
        private MemberInfo _member = null;
        private Expression _finder = null;

        public static Expression Replace(Expression expression)
        {
            var replacer = new GroupKeyReplacer();
            replacer.Visit(expression);
            return replacer._finder ?? expression;
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            if (_member == null)
            {
                _member = memberExp.Member;
            }

            Visit(memberExp.Expression);
            return memberExp;
        }

        protected override Expression VisitNew(NewExpression newExp)
        {
            if (newExp.Type.IsGenericType &&
               newExp.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
            {
                Visit(newExp.Arguments[0]);
                return newExp;
            }

            return base.VisitNew(newExp);
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (_member != null && (_member.Name == "Key" || _member.Name == column.Name))
            {
                _finder = column;
            }

            return column;
        }
    }
}
