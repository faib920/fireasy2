// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 表示不支持架构查询定义的异常。
    /// </summary>
    [Serializable]
    public sealed class SchemaQueryNotSupportedException : NotSupportedException
    {
        /// <summary>
        /// 初始化 <see cref="SchemaQueryNotSupportedException"/> 类的新实例。
        /// </summary>
        /// <param name="metadataName">元数据名称。</param>
        public SchemaQueryNotSupportedException(string metadataName)
            : base(SR.GetString(SRKind.SchemaQueryNotSupported))
        {

        }
    }
}
