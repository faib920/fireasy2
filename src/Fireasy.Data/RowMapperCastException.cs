// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data
{
    /// <summary>
    /// <see cref="IDataRowMapper"/> 读取指定名称的字段时，无法转换到相应的类型时抛出此异常。
    /// </summary>
    [Serializable]
    public class RowMapperCastException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="RowMapperCastException"/> 类的新实例。
        /// </summary>
        /// <param name="name">IDataReader 或 DataRow 中的字段名称。</param>
        /// <param name="castType">预期转换的类型。</param>
        /// <param name="innerException">内部异常信息。</param>
        public RowMapperCastException(string name, Type castType, Exception innerException)
            : base(SR.GetString(SRKind.UnableCastRowMapper, name, castType), innerException)
        {
        }
    }
}
