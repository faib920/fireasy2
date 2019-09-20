// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Data.Entity.Metadata
{
    /// <summary>
    /// 用于描述实体树结构的元数据。无法继承此类。
    /// </summary>
    public sealed class EntityTreeMetadata
    {
        private EntityTreeMappingAttribute attribute;
        private int[] lengthBits;

        internal EntityTreeMetadata(EntityMetadata parent, EntityTreeMappingAttribute attribute)
        {
            Parent = parent;
            this.attribute = attribute;

            if (attribute.SignStyle == SignStyle.Variable && attribute.SignBits != null)
            {
                lengthBits = new int[attribute.SignBits.Length];
                var total = attribute.SignBits[0];
                lengthBits[0] = total;

                for (var i = 1; i < attribute.SignBits.Length; i++)
                {
                    total += attribute.SignBits[i];
                    lengthBits[i] = total;
                }
            }
        }

        /// <summary>
        /// 获取所属的 <see cref="EntityMetadata"/>。
        /// </summary>
        public EntityMetadata Parent { get; private set; }

        /// <summary>
        /// 获取标识内部标记。
        /// </summary>
        public IProperty InnerSign { get; private set; }

        /// <summary>
        /// 获取标识级别的属性。
        /// </summary>
        public IProperty Level { get; private set; }

        /// <summary>
        /// 获取标识排序的属性。
        /// </summary>
        public IProperty Order { get; private set; }

        /// <summary>
        /// 获取标识名称的属性。
        /// </summary>
        public IProperty Name { get; private set; }

        /// <summary>
        /// 获取标识全名的属性。
        /// </summary>
        public IProperty FullName { get; private set; }

        /// <summary>
        /// 获取标识是否有孩子的属性。
        /// </summary>
        public IProperty HasChildren { get; private set; }

        /// <summary>
        /// 获取或设置全名的名称分隔符。
        /// </summary>
        public string NameSeparator { get { return attribute.NameSeparator; } }

        /// <summary>
        /// 获取或设置标记的长度。
        /// </summary>
        public int SignLength { get { return attribute.SignLength; } }

        /// <summary>
        /// 获取或设置每一级标记的位数。
        /// </summary>
        public int[] SignBits { get { return attribute.SignBits; } }

        /// <summary>
        /// 获取或设置标记的编码方式。
        /// </summary>
        public SignStyle SignStyle { get { return attribute.SignStyle; } }

        /// <summary>
        /// 通过属性初始元数据结构。
        /// </summary>
        /// <param name="property"></param>
        internal void InitTreeMetadata(IProperty property)
        {
            if (property.Name.Equals(attribute.InnerSign))
            {
                InnerSign = property;
            }
            else if (property.Name.Equals(attribute.Level))
            {
                Level = property;
            }
            else if (property.Name.Equals(attribute.Order))
            {
                Order = property;
            }
            else if (property.Name.Equals(attribute.Name))
            {
                Name = property;
            }
            else if (property.Name.Equals(attribute.FullName))
            {
                FullName = property;
            }
        }

        /// <summary>
        /// 获取下一级编码的长度。
        /// </summary>
        /// <param name="signLength">当前的编码长度。</param>
        /// <returns></returns>
        internal int GetNextLevelLength(int signLength)
        {
            if (SignStyle == SignStyle.Fixed || lengthBits == null || lengthBits.Length <= 1)
            {
                return SignLength;
            }

            var index = Array.IndexOf(lengthBits, signLength);
            if (index != -1 && index < lengthBits.Length - 1)
            {
                return SignBits[index + 1];
            }

            return SignLength;
        }

        /// <summary>
        /// 获取上一级编码的长度。
        /// </summary>
        /// <param name="signLength">当前的编码长度。</param>
        /// <returns></returns>
        internal int GetCurrentLevelLength(int signLength)
        {
            if (SignStyle == SignStyle.Fixed || lengthBits == null || lengthBits.Length <= 1)
            {
                return SignLength;
            }

            var index = Array.IndexOf(lengthBits, signLength);
            if (index != -1)
            {
                return SignBits[index];
            }

            return SignLength;
        }

        /// <summary>
        /// 获取上一级编码的长度。
        /// </summary>
        /// <param name="signLength">当前的编码长度。</param>
        /// <returns></returns>
        internal int GetPreviousLevelLength(int signLength)
        {
            if (SignStyle == SignStyle.Fixed || lengthBits == null || lengthBits.Length <= 1)
            {
                return SignLength;
            }

            var index = Array.IndexOf(lengthBits, signLength);
            if (index > 0)
            {
                return SignBits[index - 1];
            }

            return SignLength;
        }
    }
}