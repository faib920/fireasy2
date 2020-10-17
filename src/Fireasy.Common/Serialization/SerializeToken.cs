// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 序列化令牌，用于在序列化和反序列化过程中验证二进制数据。
    /// </summary>
    public sealed class SerializeToken
    {
        /// <summary>
        /// 获取或设置序列令牌的数据。
        /// </summary>
        public byte[] Data { get; set; }
    }
}
