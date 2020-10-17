// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data
{
    /// <summary>
    /// 定义数据排序。
    /// </summary>
    public class SortDefinition
    {
        public readonly static SortDefinition Empty = new SortDefinition();

        /// <summary>
        /// 获取或设置排序顺序。
        /// </summary>
        public SortOrder Order { get; set; }

        /// <summary>
        /// 获取或设置排序的成员名称。
        /// </summary>
        public string Member { get; set; }

        /// <summary>
        /// 使用一组值替换 Member 属性。
        /// </summary>
        /// <param name="members">一个字典，Key 与 Member 属性相匹配，使用 Value 做替换。</param>
        /// <returns></returns>
        public SortDefinition Replace(Dictionary<string, string> members)
        {
            if (string.IsNullOrEmpty(Member))
            {
                return this;
            }

            if (members.ContainsKey(Member))
            {
                Member = members[Member];
            }

            return this;
        }

        /// <summary>
        /// 使用一组值替换 Member 属性。
        /// </summary>
        /// <param name="members">一组偶数个数的字符串，下标为偶数(包括0)的字符串与 Member 作匹配，使用其后一个字符串进行替换。</param>
        /// <returns></returns>
        public SortDefinition Replace(params string[] members)
        {
            Guard.ArgumentNull(members, nameof(members));

            var len = members.Length;
            if (len % 2 != 0)
            {
                throw new ArgumentException();
            }

            for (var i = 0; i < len; i += 2)
            {
                if (members[i] == Member)
                {
                    Member = members[i + 1];
                    break;
                }
            }

            return this;
        }

        /// <summary>
        /// 将 <paramref name="member"/> 替换为具体的成员。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="member">成员名称。</param>
        /// <param name="expression">一个 <see cref="MemberExpression"/>。</param>
        /// <returns></returns>
        public SortDefinition Replace<TSource>(string member, Expression<Func<TSource, object>> expression)
        {
            if (Member == member)
            {
                Member = MemberFinder.Find(expression);
            }

            return this;
        }

        public override string ToString()
        {
            return string.Format("Member:{0},Order:{1}", Member, Order);
        }

        private class MemberFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private readonly List<string> names = new List<string>();

            public static string Find(Expression expression)
            {
                var finder = new MemberFinder();
                finder.Visit(expression);
                return string.Join(".", finder.names);
            }

            protected override Expression VisitMember(MemberExpression mbrExp)
            {
                Visit(mbrExp.Expression);
                names.Add(mbrExp.Member.Name);
                return mbrExp;
            }
        }
    }
}
