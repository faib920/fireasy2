// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 提供对主题名称标准化的接口。
    /// </summary>
    public interface ITopicNameNormalizer
    {
        /// <summary>
        /// 标准化主题名称。
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        string NormalizeName(string topicName);
    }
}