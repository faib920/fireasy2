// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Syntax;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// ELinq 表达式的翻译结果。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class TranslateResult
    {
        internal ISyntaxProvider Syntax;

        /// <summary>
        /// 获取翻译的查询文本。
        /// </summary>
        public string QueryText
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取参数集合。
        /// </summary>
        public ParameterCollection Parameters
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取数据分段对象。
        /// </summary>
        public IDataSegment DataSegment
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取嵌套结果列表。
        /// </summary>
        public ReadOnlyCollection<TranslateResult> NestedResults
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            var queryText = QueryText;

            if (DataSegment != null)
            {
                queryText = Syntax.Segment(queryText, DataSegment);
            }

            var sb = new StringBuilder(queryText);

            if (Parameters.Count > 0)
            {
                sb.AppendLine();

                foreach (var par in Parameters)
                {
                    //字符或日期型，加'
                    if (par.Value is string || par.Value is DateTime || par.Value is char)
                    {
                        sb.AppendFormat("\n{0}='{1}'", par.ParameterName, par.Value);
                    }

                    //字节数组，转换为字符串
                    else if (par.Value is byte[])
                    {
                        sb.AppendFormat("\n{0}='{1}'", par.ParameterName, Encoding.ASCII.GetString(par.Value as byte[]));
                    }
                    else
                    {
                        sb.AppendFormat("\n{0}={1}", par.ParameterName, par.Value);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
