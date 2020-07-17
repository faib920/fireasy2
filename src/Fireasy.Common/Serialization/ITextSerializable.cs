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
    /// 提供对象文本格式化的序列化和反序列化方法。
    /// </summary>
    public interface ITextSerializable
    {
        /// <summary>
        /// 将对象序列为文本表示。
        /// </summary>
        /// <param name="serializer">一个 <see cref="ITextSerializer"/> 对象。</param>
        /// <returns>表示对象的文本。</returns>
        string Serialize(ITextSerializer serializer);

        /// <summary>
        /// 将文本反序列化为对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="ITextSerializer"/> 对象。</param>
        /// <param name="text">表示对象的文本。</param>
        void Deserialize(ITextSerializer serializer, string text);
    }
}
