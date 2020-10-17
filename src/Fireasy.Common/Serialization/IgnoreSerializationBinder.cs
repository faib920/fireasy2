// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 一个序列化控制类，用于忽略二进制反序列化过程中对类型的转换。
    /// </summary>
    public sealed class IgnoreSerializationBinder : SerializationBinder
    {
        /// <summary>
        /// 根据提供的程序集和类名称，转换目标类型。
        /// </summary>
        /// <param name="assemblyName">程序集名称。</param>
        /// <param name="typeName">类的名称。</param>
        /// <returns>绑定的类型。</returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            var index = assemblyName.IndexOf(",");
            var version = assemblyName.Substring(index);
            assemblyName = assemblyName.Substring(0, index);
            return Type.GetType(typeName.Replace(version, string.Empty) + ", " + assemblyName);
        }
    }
}
