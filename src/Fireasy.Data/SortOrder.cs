// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using System;
using System.Collections.Generic;

namespace Fireasy.Data
{
    /// <summary>
    /// 排序顺序。
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// 未定义。
        /// </summary>
        None,
        /// <summary>
        /// 升序。
        /// </summary>
        Ascending,
        /// <summary>
        /// 降序。
        /// </summary>
        Descending
    }

    /// <summary>
    /// 定义数据排序。
    /// </summary>
    public class SortDefinition
    {
        public readonly static SortDefinition Empty = new SortDefinition();

        /// <summary>
        /// 获取或设置排序顺序。
        /// </summary>
        public SortOrder Order { get; set; }

        /// <summary>
        /// 获取或设置排序的成员名称。
        /// </summary>
        public string Member { get; set; }

        /// <summary>
        /// 使用一组值替换 Member 属性。
        /// </summary>
        /// <param name="members">一个字典，Key 与 Member 属性相匹配，使用 Value 做替换。</param>
        /// <returns></returns>
        public SortDefinition Replace(Dictionary<string, string> members)
        {
            if (string.IsNullOrEmpty(Member))
            {
                return this;
            }

            if (members.ContainsKey(Member))
            {
                Member = members[Member];
            }

            return this;
        }

        /// <summary>
        /// 使用一组值替换 Member 属性。
        /// </summary>
        /// <param name="members">一组偶数个数的字符串，下标为偶数(包括0)的字符串与 Member 作匹配，使用其后一个字符串进行替换。</param>
        /// <returns></returns>
        public SortDefinition Replace(params string[] members)
        {
            Guard.ArgumentNull(members, nameof(members));

            if (members.Length % 2 != 0)
            {
                throw new ArgumentException();
            }

            for (var i = 0; i < members.Length; i+= 2)
            {
                if (members[i] == Member)
                {
                    Member = members[i + 1];
                    break;
                }
            }

            return this;
        }

        public override string ToString()
        {
            return string.Format("Member:{0},Order:{1}", Member, Order);
        }
    }
}
