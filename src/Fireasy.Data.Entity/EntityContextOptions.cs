// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Initializers;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// <see cref="EntityContext"/> 的参数。
    /// </summary>
    public sealed class EntityContextOptions
    {
        /// <summary>
        /// 初始化 <see cref="EntityContextOptions"/> 类的新实例。
        /// </summary>
        public EntityContextOptions()
        {
            Initializers = new EntityContextPreInitializerCollection();
            Initializers.Add<RecompileAssemblyPreInitializer>();
        }

        /// <summary>
        /// 初始化 <see cref="EntityContextOptions"/> 类的新实例。
        /// </summary>
        /// <param name="configName">实例名称。</param>
        public EntityContextOptions(string configName)
            : this()
        {
            ConfigName = configName;
        }

        /// <summary>
        /// 获取或设置是否通知持久化事件。默认为 false。
        /// </summary>
        public bool NotifyEvents { get; set; } = false;

        /// <summary>
        /// 获取或设置是否验证实体属性。默认为 true。
        /// </summary>
        public bool ValidateEntity { get; set; } = true;

        /// <summary>
        /// 获取实例名称。
        /// </summary>
        public string ConfigName { get; private set; }

        /// <summary>
        /// 获取或设置 <see cref="EntityContextInitializeContext"/> 实例创建工厂。
        /// </summary>
        public Func<EntityContextInitializeContext> ContextFactory { get; set; }

        /// <summary>
        /// 获取初始化方法。
        /// </summary>
        public EntityContextPreInitializerCollection Initializers { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var op = (EntityContextOptions)obj;
            return NotifyEvents == op.NotifyEvents &&
                ValidateEntity == op.ValidateEntity &&
                ConfigName == op.ConfigName;
        }
    }
}
