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
    /// 无法正常获取架构信息时抛出的异常。
    /// </summary>
    [Serializable]
    public sealed class SchemaException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="SchemaException"/> 类的新实例。
        /// </summary>
        /// <param name="collectionName">架构类别的名称。</param>
        /// <param name="innerExp">内部异常。</param>
        public SchemaException(string collectionName, Exception innerExp)
            : base(SR.GetString(SRKind.SchemaGetFaild, collectionName), innerExp)
        {
        }
    }
}
