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
    /// 实体树的节点数据。无法继承此类。
    /// </summary>
    public sealed class EntityTreeUpdatingBag
    {
        /// <summary>
        /// 获取内码值。
        /// </summary>
        public string InnerId { get; internal set; }

        /// <summary>
        /// 获取级别值。
        /// </summary>
        public int Level { get; internal set; }

        /// <summary>
        /// 获取排序值。
        /// </summary>
        public int Order { get; internal set; }

        /// <summary>
        /// 获取名称值。
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 获取全名值。
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