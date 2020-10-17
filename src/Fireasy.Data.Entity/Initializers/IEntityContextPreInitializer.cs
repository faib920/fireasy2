// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data.Entity.Initializers
{
    /// <summary>
    /// 实体上下文初始化接口。
    /// </summary>
    public interface IEntityContextPreInitializer
    {
        /// <summary>
        /// 初始化操作。
        /// </summary>
        /// <param name="context"></param>
        void PreInitialize(EntityContextPreInitializeContext context);
    }
}
