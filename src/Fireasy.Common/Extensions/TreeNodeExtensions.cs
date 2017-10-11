// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Common.Extensions
{
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// 逐级找出子节点。
        /// </summary>
        /// <param name="nodes">当前的节点集合。</param>
        /// <param name="parents">父节点ID集合。</param>
        /// <param name="factory">生成节点的函数。</param>
        /// <param name="index">控制从后向前推进的索引值。</param>
        public static void Expand<TSource, TValue>(this List<TSource> nodes, List<TValue> parents, Func<TValue, IEnumerable<TSource>> factory, int index)
            where TSource : ITreeNode<TSource>
            where TValue : IEquatable<TValue>
        {
            TreeNodeExpandScope control = null;

            if (parents.Count == 0 || index < 0)
            {
                return;
            }

            if (TreeNodeExpandScope.Current == null)
            {
                control = new TreeNodeExpandScope();
            }

            try
            {
                foreach (var node in nodes)
                {
                    if (parents[index].Equals((TValue)node.Id))
                    {
                        node.Children = factory((TValue)node.Id).ToList();
                        node.IsLoaded = true;
                        Expand(node.Children, parents, factory, index - 1);
                    }
                }
            }
            finally
            {
                if (control != null)
                {
                    control.Dispose();
                }
            }
        }
    }

    internal class TreeNodeExpandScope : Scope<TreeNodeExpandScope>
    {
    }

    /// <summary>
    /// 用于检查是否在线程内递归调用 TreeNodeExtensions.Expand 方法，有利于减少不必要的数据操作。
    /// </summary>
    public class TreeNodeExpandChecker
    {
        /// <summary>
        /// 判断是否使用过 TreeNodeExtensions.Expand 方法。
        /// </summary>
        /// <returns></returns>
        public static bool IsExpanded()
        {
            return TreeNodeExpandScope.Current != null;
        }
    }
}
