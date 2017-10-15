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

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// 树结构转换器，动态给定需要返回的字段。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicTreeNodeJsonConverter<T> : TreeNodeJsonConverter<T> where T : ITreeNode
    {
        /// <summary>
        /// 初始化 <see cref="DynamicTreeNodeJsonConverter"/> 类的新实例。
        /// </summary>
        /// <param name="textExp">作为 text 的成员表达式。</param>
        /// <param name="attExps">其他附加的成员表达式。</param>
        public DynamicTreeNodeJsonConverter(Expression<Func<T, object>> textExp, params Expression<Func<T, object>>[] attExps)
            : base(GetProperties(textExp, attExps))
        {
        }

        private static Dictionary<string, Func<T, object>> GetProperties(Expression<Func<T, object>> textExp, params Expression<Func<T, object>>[] attExps)
        {
            var dict = new Dictionary<string, Func<T, object>>();

            if (textExp != null)
            {
                if (textExp is LambdaExpression lambda)
                {
                    if (lambda.Body is MemberExpression mbrExp)
                    {
                        var func = (Func<T, object>)lambda.Compile();
                        dict.Add("text", func);
                        dict.Add(mbrExp.Member.Name, func);
                    }
                }
            }

            foreach (var expression in attExps)
            {
                if (expression is LambdaExpression lambda)
                {
                    if (lambda.Body is MemberExpression mbrExp)
                    {
                        var func = (Func<T, object>)lambda.Compile();
                        dict.Add(mbrExp.Member.Name, func);
                    }
                }
            }

            return dict;
        }
    }
}
