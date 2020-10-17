// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 表示树节点。
    /// </summary>
    public interface ITreeNode
    {
        /// <summary>
        /// 获取表示节点的唯一编码。
        /// </summary>
        object Id { get; }

        /// <summary>
        /// 获取或设置是否有子节点。
        /// </summary>
        bool HasChildren { get; set; }

        /// <summary>
        /// 获取或设置是否加载了子节点列表。
        /// </summary>
        bool IsLoaded { get; set; }

        /// <summary>
        /// 获取或设置子节点列表。
        /// </summary>
        IList Children { get; set; }
    }

    /// <summary>
    /// 表示树节点。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeNode<T> : ITreeNode
    {
        /// <summary>
        /// 获取或设置子节点列表。
        /// </summary>
        new List<T> Children { get; set; }
    }

    /// <summary>
    /// 表示树节点，Id 的类型可以指定。
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface ITreeNode<TNode, TId> : ITreeNode<TNode>
    {
        /// <summary>
        /// 获取表示节点的唯一编码。
        /// </summary>
        new TId Id { get; }
    }
}
