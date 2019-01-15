// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace Fireasy.Web.Mvc
{
    public class MvcOptions
    {
        /// <summary>
        /// 获取或设置是否 Fireasy 提供的 Json 序列化来格式化返回值。默认为 true。
        /// </summary>
        public bool UseTypicalJsonSerializer { get; set; } = true;

        /// <summary>
        /// 获取或设置是否替换 <see cref="MetadataReferenceFeatureProvider"/>。默认为 false。
        /// </summary>
        [Obsolete]
        public bool UseReferenceAssembly { get; set; } = false;

        /// <summary>
        /// 获取或设置是否使用 <see cref="JsonModelBinder"/>。默认为 true。
        /// </summary>
        public bool UseJsonModelBinder { get; set; } = true;

        /// <summary>
        /// 获取或设置是否使用根级 Razor 视图搜索。默认为 true。
        /// </summary>
        public bool UseRootRazorProject { get; set; } = true;

        /// <summary>
        /// 获取或设置是否禁用 <see cref="IObjectModelValidator"/> 验证。默认为 true。
        /// </summary>
        public bool DisableModelValidator { get; set; } = true;

        /// <summary>
        /// 获取或设置 Json 序列化选项。
        /// </summary>
        public JsonSerializeOption JsonSerializeOption { get; set; } = new JsonSerializeOption();
    }
}
#endif