// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// ʵ�����Ľڵ����ݡ��޷��̳д��ࡣ
    /// </summary>
    public sealed class EntityTreeUpdatingBag
    {
        /// <summary>
        /// ��ȡ����ֵ��
        /// </summary>
        public string InnerId { get; internal set; }

        /// <summary>
        /// ��ȡ����ֵ��
        /// </summary>
        public int Level { get; internal set; }

        /// <summary>
        /// ��ȡ����ֵ��
        /// </summary>
        public int Order { get; internal set; }

        /// <summary>
        /// ��ȡ����ֵ��
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// ��ȡȫ��ֵ��
        /// </summary>
        public string FullName { get; internal set; }

        internal EntityTreeUpdatingBag Clone()
        {
            return new EntityTreeUpdatingBag
            {
                Level = Level,
                Order = Order,
                InnerId = InnerId,
                Name = Name,
                FullName = FullName
            };
        }
    }

    public class EntityTreeUpfydatingArgument
    {
        public EntityTreeUpdatingBag OldValue { get; set; }

        public EntityTreeUpdatingBag NewValue { get; set; }
    }

}