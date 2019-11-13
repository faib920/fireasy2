// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Localization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 用于在窗体上扩展字符串本地化。
    /// </summary>
    [ProvideProperty("ResourceKey", typeof(Component))]
    public class StringLocalizerHolder : Component, IExtenderProvider
    {
        private Dictionary<Component, string> keys = new Dictionary<Component, string>();

        public StringLocalizerHolder()
        {
        }

        public StringLocalizerHolder(IContainer container)
            : this()
        {
            container.Add(this);
        }

        /// <summary>
        /// 获取或设置字符串本地化的配置。
        /// </summary>
        public string Name { get; set; }

        public bool CanExtend(object extendee)
        {
            return typeof(Component).IsAssignableFrom(extendee.GetType());
        }

        public string GetResourceKey(Component control)
        {
            if (keys.ContainsKey(control))
            {
                return keys[control];
            }

            return string.Empty;
        }

        public void SetResourceKey(Component control, string name)
        {
            if (keys.ContainsKey(control))
            {
                if (string.IsNullOrEmpty(name))
                {
                    keys.Remove(control);
                }
                else
                {
                    keys[control] = name;
                }
            }
            else if (!string.IsNullOrEmpty(name))
            {
                keys.Add(control, name);
            }
        }

        /// <summary>
        /// 应用字符串本地化。
        /// </summary>
        public void Apply()
        {
            var manager = StringLocalizerFactory.CreateManager();
            if (manager == null || string.IsNullOrEmpty(Name))
            {
                return;
            }

            var localizer = manager.GetLocalizer(Name);

            foreach (var kvp in keys)
            {
                if (kvp.Key is Control control)
                {
                    control.Text = localizer[kvp.Value];
                }
                else if (kvp.Key is TreeListColumn column)
                {
                    column.Text = localizer[kvp.Value];
                }
            }
        }
    }
}
