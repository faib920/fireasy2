// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 指定需要排除的类属性，序列化时将不输出这些属性。
    /// </summary>
    public class JsonFilterConverter<T> : JsonConverter<T>
    {
        private readonly string[] _exclusiveNames = null;

        /// <summary>
        /// 初始化 <see cref="JsonFilterConverter"/> 类的新实例。
        /// </summary>
        /// <param name="filterExps"></param>
        public JsonFilterConverter(params Expression<Func<T, object>>[] filterExps)
        {
            _exclusiveNames = filterExps == null ? null : ExclusiveNameFinder.Find(filterExps);
        }

        /// <summary>
        /// 不支持支序列化。
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// 将对象写为 Json 文本。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="writer"><see cref="JsonWriter"/>对象。</param>
        /// <param name="obj">要序列化的 <see cref="DateTime"/> 值。</param>
        public override void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
        {
            if (_exclusiveNames == null)
            {
                serializer.Serialize(obj);
            }
            else
            {
                serializer.Option.ExclusiveNames = _exclusiveNames;
                serializer.Option.Converters.RemoveAll(s => s is JsonFilterConverter<T>);
                serializer.Serialize(obj);
            }
        }

        private class ExclusiveNameFinder : Linq.Expressions.ExpressionVisitor
        {
            private string propertyName;

            public static string[] Find(params Expression<Func<T, object>>[] filterExps)
            {
                var result = new List<string>();

                foreach (var exp in filterExps)
                {
                    var finder = new ExclusiveNameFinder();
                    finder.Visit(exp);
                    if (!string.IsNullOrEmpty(finder.propertyName))
                    {
                        result.Add(finder.propertyName);
                    }
                }

                return result.ToArray();
            }

            protected override Expression VisitMember(MemberExpression memberExp)
            {
                propertyName = memberExp.Member.Name;
                return memberExp;
            }
        }
    }
}
