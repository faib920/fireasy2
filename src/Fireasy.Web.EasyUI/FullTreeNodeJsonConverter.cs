// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// 树结构转换器，默认将所有属性返回到 Json 中。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FullTreeNodeJsonConverter<T> : TreeNodeJsonConverter<T> where T : ITreeNode
    {
        private static SafetyDictionary<Type, Dictionary<string, Func<T, object>>> cache = new SafetyDictionary<Type, Dictionary<string, Func<T, object>>>();

        /// <summary>
        /// 初始化 <see cref="FullTreeNodeJsonConverter"/> 类的新实例。
        /// </summary>
        /// <param name="textExp">作为 text 的成员表达式。</param>
        public FullTreeNodeJsonConverter(Expression<Func<T, object>> textExp)
            : base(GetProperties(textExp))
        {
        }

        private static Dictionary<string, Func<T, object>> GetProperties(Expression<Func<T, object>> textExp)
        {
            var result = cache.GetOrAdd(typeof(T), key =>
                {
                    var parExp = Expression.Parameter(key, "s");

                    var dict = new Dictionary<string, Func<T, object>>();

                    foreach (var property in key.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var proExp = Expression.Property(parExp, property);
                        var cvtExp = Expression.Convert(proExp, typeof(object));
                        var lambda = Expression.Lambda<Func<T, object>>(cvtExp, parExp);
                        var func = lambda.Compile();
                        dict.Add(property.Name, func);
                    }

                    return dict;
                });

            if (textExp != null)
            {
                if (textExp is LambdaExpression lambda)
                {
                    if (lambda.Body is MemberExpression mbrExp)
                    {
                        var func = (Func<T, object>)lambda.Compile();
                        result.Add("text", func);
                    }
                }
            }

            return result;
        }
    }
}
