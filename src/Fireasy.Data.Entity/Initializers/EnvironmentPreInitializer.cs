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
    /// 此初始化器为当前的上下文附加一个 <see cref="EntityPersistentEnvironment"/> 对象。
    /// </summary>
    public class EnvironmentPreInitializer : IEntityContextPreInitializer
    {
        public EntityPersistentEnvironment Environment { get; set; }

        void IEntityContextPreInitializer.PreInitialize(EntityContextPreInitializeContext context)
        {
            context.Service.InitializeEnvironment(Environment);
        }
    }
}
