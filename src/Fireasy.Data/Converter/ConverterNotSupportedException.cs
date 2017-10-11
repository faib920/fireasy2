// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 当强类型与数据库数据类型的转换不支持时抛出的异常。
    /// </summary>
    [Serializable]
    public class ConverterNotSupportedException : NotSupportedException
    {
        /// <summary>
        /// 初始化 <see cref="ConverterNotSupportedException"/> 类的新实例。
        /// </summary>
        /// <param name="type">对象的类型。</param>
        /// <param name="dbType">数据库数据类型。</param>
        public ConverterNotSupportedException(Type type, DbType dbType)
            : base (SR.GetString(SRKind.ConverterNotSupported, type.Name, dbType))
        {
        }
    }
}
