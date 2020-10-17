// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Web.EasyUI.Binders
{
    /// <summary>
    ///  <see cref="ISettingsBindable"/> 的绑定者。
    /// </summary>
    public interface ISettingsBinder
    {
        /// <summary>
        /// 判断 <paramref name="settings"/> 是否可应用此绑定规则。
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        bool CanBind(ISettingsBindable settings);

        /// <summary>
        /// 使用类型及属性对 <paramref name="settings"/> 进行绑定。
        /// </summary>
        /// <param name="modelType">模型类型。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="settings"></param>
        void Bind(Type modelType, string propertyName, ISettingsBindable settings);
    }
}
