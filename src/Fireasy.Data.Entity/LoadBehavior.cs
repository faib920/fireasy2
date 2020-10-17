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
    /// 关系型属性的加载行为。
    /// </summary>
    public enum LoadBehavior
    {
        /// <summary>
        /// 程序不做任何的操作，人为进行加载。
        /// </summary>
        None,
        /// <summary>
        /// 在实体初始化时，始终加载关系型属性对象，数量较大时应谨慎使用，并注意两个实体间是否存在相互关联的情况。
        /// </summary>
        Always,
        /// <summary>
        /// 默认行为，要首次使用关系型属性时，才进行延迟加载。
        /// </summary>
        Lazy
    }
}
