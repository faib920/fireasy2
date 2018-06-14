// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 序列化选项。
    /// </summary>
    public class SerializeOption
    {
        /// <summary>
        /// 获取或设置是否对属性名使用 Camel 语法命名规则。默认为 false。
        /// </summary>
        public bool CamelNaming { get; set; }

        /// <summary>
        /// 获取或设置可包含的属性名数组。
        /// </summary>
        public string[] InclusiveNames { get; set; }

        /// <summary>
        /// 获取或设置要排除的属性名数组。
        /// </summary>
        public string[] ExclusiveNames { get; set; }

        /// <summary>
        /// 获取或设置可包含的属性名数组。
        /// </summary>
        public List<MemberInfo> InclusiveMembers { get; set; }

        /// <summary>
        /// 获取或设置要排除的属性名数组。
        /// </summary>
        public List<MemberInfo> ExclusiveMembers { get; set; }

        /// <summary>
        /// 获取或设置序列化的转换器。
        /// </summary>
        public ConverterList Converters { get; set; } = new ConverterList();

        /// <summary>
        /// 获取或设置是否忽略 <see cref="Type"/> 类型的属性。默认为 true。
        /// </summary>
        public bool IgnoreType { get; set; } = true;

        /// <summary>
        /// 获取或设置是否缩进。默认为 true。
        /// </summary>
        public bool Indent { get; set; } = true;

        /// <summary>
        /// 使用表达式指定在序列化 <typeparamref name="T"/> 时仅被序列化的成员列表。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions">一组 <see cref="MemberExpression"/> 表达式。</param>
        /// <returns></returns>
        public SerializeOption Include<T>(params Expression<Func<T, object>>[] expressions)
        {
            if (expressions != null && expressions.Length > 0)
            {
                var members = ResolveExpressions(expressions);
                if (!members.IsNullOrEmpty())
                {
                    if (InclusiveMembers == null)
                    {
                        InclusiveMembers = new List<MemberInfo>(members);
                    }
                    else
                    {
                        InclusiveMembers.AddRange(members);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// 使用表达式指定在序列化 <typeparamref name="T"/> 时要排除序列化的成员列表。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions">一组 <see cref="MemberExpression"/> 表达式。</param>
        /// <returns></returns>
        public SerializeOption Exclude<T>(params Expression<Func<T, object>>[] expressions)
        {
            if (expressions != null && expressions.Length > 0)
            {
                var members = ResolveExpressions(expressions);
                if (!members.IsNullOrEmpty())
                {
                    if (ExclusiveMembers == null)
                    {
                        ExclusiveMembers = new List<MemberInfo>(members);
                    }
                    else
                    {
                        ExclusiveMembers.AddRange(members);
                    }
                }
            }

            return this;
        }

        private IEnumerable<MemberInfo> ResolveExpressions<T>(Expression<Func<T, object>>[] expressions)
        {
            foreach (LambdaExpression exp in expressions)
            {
                if (exp.Body is MemberExpression mbrExp)
                {
                    yield return mbrExp.Member;
                }
                else if (exp.Body is UnaryExpression uryExp && uryExp.Operand is MemberExpression mbrExp1)
                {
                    yield return mbrExp1.Member;
                }
            }
        }
    }
}
