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
using System.Globalization;
using System.Linq;
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
        /// 初始化 <see cref="SerializeOption"/> 类的新实例。
        /// </summary>
        public SerializeOption()
        {
            ContractResolver = new DefaultContractResolver(this);
        }

        static SerializeOption()
        {
            GlobalConverters = new ConverterList();
        }

        /// <summary>
        /// 全局的转换器。
        /// </summary>
        public static ConverterList GlobalConverters { get; private set; }

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
        /// 获取或设置序列化契约解析器。
        /// </summary>
        public IContractResolver ContractResolver { get; set; }

        /// <summary>
        /// 获取或设置是否缩进。默认为 true。
        /// </summary>
        public bool Indent { get; set; } = true;

        /// <summary>
        /// 获取或设置命名的处理方式。
        /// </summary>
        public NamingHandling NamingHandling { get; set; }

        /// <summary>
        /// 获取或设置空值的处理方式。
        /// </summary>
        public NullValueHandling NullValueHandling { get; set; }

        /// <summary>
        /// 获取或设置循环引用时如何处理。
        /// </summary>
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; }

        /// <summary>
        /// 获取或设置日期如何序列化。
        /// </summary>
        public DateFormatHandling DateFormatHandling { get; set; }

        /// <summary>
        /// 获取或设置日期的时区处理方式。
        /// </summary>
        public DateTimeZoneHandling DateTimeZoneHandling { get; set; }

        /// <summary>
        /// 获取或设置特定区域性。
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// 附加另一个选项的转换器。
        /// </summary>
        /// <param name="other"></param>
        public virtual void AttachConverters(SerializeOption other)
        {
            Converters.AddRange(other.Converters);
        }

        /// <summary>
        /// 引用另一个选项的设置属性。
        /// </summary>
        /// <param name="other"></param>
        public virtual void Reference(SerializeOption other)
        {
            NamingHandling = other.NamingHandling;
            Converters.AddRange(other.Converters);
            Indent = other.Indent;
            ReferenceLoopHandling = other.ReferenceLoopHandling;
            DateFormatHandling = other.DateFormatHandling;
            DateTimeZoneHandling = other.DateTimeZoneHandling;
            NullValueHandling = other.NullValueHandling;

            if (other.InclusiveNames != null)
            {
                InclusiveNames = InclusiveNames == null ? other.InclusiveNames : InclusiveNames.Union(other.InclusiveNames).Distinct().ToArray();
            }

            if (other.ExclusiveNames != null)
            {
                ExclusiveNames = ExclusiveNames == null ? other.ExclusiveNames : ExclusiveNames.Union(other.ExclusiveNames).Distinct().ToArray();
            }

            if (other.InclusiveMembers != null)
            {
                if (InclusiveMembers != null)
                {
                    InclusiveMembers.AddRange(other.InclusiveMembers);
                }
                else
                {
                    InclusiveMembers = other.InclusiveMembers;
                }
            }

            if (other.ExclusiveMembers != null)
            {
                if (ExclusiveMembers != null)
                {
                    ExclusiveMembers.AddRange(other.ExclusiveMembers);
                }
                else
                {
                    ExclusiveMembers = other.ExclusiveMembers;
                }
            }
        }

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

    /// <summary>
    /// 循环引用时的处理策略。
    /// </summary>
    public enum ReferenceLoopHandling
    {
        /// <summary>
        /// 不做检查。
        /// </summary>
        None,
        /// <summary>
        /// 抛出异常。
        /// </summary>
        Error,
        /// <summary>
        /// 忽略，不再序列化。
        /// </summary>
        Ignore
    }

    /// <summary>
    /// 日期格式的处理。
    /// </summary>
    public enum DateFormatHandling
    {
        /// <summary>
        /// 缺省格式，如 "2010-12-01"。
        /// </summary>
        Default,
        /// <summary>
        /// ISO 格式，如 "2012-03-21T05:40Z"。
        /// </summary>
        IsoDateFormat,
        /// <summary>
        /// Micrisoft 格式，如 "\/Date(1198908717056)\/"。
        /// </summary>
        MicrosoftDateFormat
    }

    /// <summary>
    /// 日期格式的时区处理。
    /// </summary>
    public enum DateTimeZoneHandling
    {
        /// <summary>
        /// 本地时间。
        /// </summary>
        Local,
        /// <summary>
        /// UTC 时间。
        /// </summary>
        Utc,
        /// <summary>
        /// 未定。
        /// </summary>
        Unspecified
    }

    /// <summary>
    /// 空值的处理方式。
    /// </summary>
    public enum NullValueHandling
    {
        /// <summary>
        /// 仍然输出 null。
        /// </summary>
        Include,
        /// <summary>
        /// 忽略该值。
        /// </summary>
        Ignore,
        /// <summary>
        /// 缺省值。
        /// </summary>
        Empty
    }

    /// <summary>
    /// 命名的处理方式。
    /// </summary>
    public enum NamingHandling
    {
        Default,
        Camel,
    }
}
