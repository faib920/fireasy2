// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common.Serialization;
using System.Collections.Generic;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 公共的设置项。
    /// </summary>
    public class GlobalSetting
    {
        /// <summary>
        /// 获取公共的文本转换器列表。
        /// </summary>
        public static List<ITextConverter> Converters { get; private set; } = new List<ITextConverter>();
    }
}
#endif