// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Fireasy.Web.EasyUI.Binders
{
    /// <summary>
    /// 提供对 <see cref="ISettingsBindable"/> 绑定的管理。
    /// </summary>
    public class SettingsBindManager
    {
        private static ConcurrentDictionary<string, ISettingsBinder> binders = new ConcurrentDictionary<string, ISettingsBinder>();

        /// <summary>
        /// 注册一个 <see cref="ISettingsBinder"/> 对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">关键名称。</param>
        /// <param name="binder"><see cref="SettingsBase"/> 绑定者。</param>
        public static void RegisterBinder<T>(string name, T binder) where T : ISettingsBinder
        {
            binders.TryAdd(name, binder);
        }

        /// <summary>
        /// 使用类型及属性对 <paramref name="settings"/> 进行绑定。
        /// </summary>
        /// <param name="modelType">模型类型。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="settings"></param>
        public static void Bind(Type modelType, string propertyName, ISettingsBindable settings)
        {
            foreach (var binder in binders.Where(s => s.Value.CanBind(settings)))
            {
                binder.Value.Bind(modelType, propertyName, settings);
            }
        }
    }
}
