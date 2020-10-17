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
    /// 提供对 <see cref="SettingsBase"/> 的额外绑定。
    /// </summary>
    public interface ISettingsBindable
    {
        /// <summary>
        /// 使用类型及属性对 Setting 进行绑定。
        /// </summary>
        /// <param name="modelType">模型类型。</param>
        /// <param name="propertyName">属性名称。</param>
        void Bind(Type modelType, string propertyName);
    }
}
