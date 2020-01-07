// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 定义一个提供 Hash 键值的对象。
    /// </summary>
    public interface IHashKeyObject
    {
        /// <summary>
        /// 获取键值。
        /// </summary>
        object Key { get; }
    }
}
