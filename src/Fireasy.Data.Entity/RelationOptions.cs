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
    /// 关系属性的选项。
    /// </summary>
    public class RelationOptions
    {
        /// <summary>
        /// 缺省的关联属性选项。
        /// </summary>
        public static readonly RelationOptions Default = new RelationOptions(LoadBehavior.Lazy);

        /// <summary>
        /// 初始化 <see cref="RelationOptions"/> 类的新实例。
        /// </summary>
        public RelationOptions()
        {
        }

        /// <summary>
        /// 初始化 <see cref="RelationOptions"/> 类的新实例。
        /// </summary>
        /// <param name="loadBehavior">加载行为。</param>
        public RelationOptions(LoadBehavior loadBehavior)
        {
            LoadBehavior = loadBehavior;
        }

        /// <summary>
        /// 获取或设置属性的加载行为。
        /// </summary>
        public LoadBehavior LoadBehavior { get; set; }

    }
}
