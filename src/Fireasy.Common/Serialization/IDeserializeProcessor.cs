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
    /// 反序列化处理器。
    /// </summary>
    public interface IDeserializeProcessor
    {
        /// <summary>
        /// 反序列化之前的处理。
        /// </summary>
        void PreDeserialize();

        /// <summary>
        /// 设置属性的值。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="value">值。</param>
        /// <returns>如果设置成功，返回 true，反之为 false。</returns>
        bool SetValue(string name, object value);

        /// <summary>
        /// 反序列化之后的处理。
        /// </summary>
        void PostDeserialize();
    }
}
