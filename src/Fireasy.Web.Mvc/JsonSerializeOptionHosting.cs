// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Fireasy.Common.Serialization;
using Microsoft.Extensions.Options;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 用于在 Action 执行期间附加 <see cref="JsonSerializeOption"/> 参数设置。
    /// </summary>
    public class JsonSerializeOptionHosting
    {
        /// <summary>
        /// 初始化 <see cref="JsonSerializeOptionHosting"/> 类的新实例。
        /// </summary>
        /// <param name="options"></param>
        public JsonSerializeOptionHosting(IOptions<MvcOptions> options)
        {
            Option = new JsonSerializeOption(options.Value.JsonSerializeOption);
        }

        /// <summary>
        /// 获取 <see cref="JsonSerializeOption"/> 对象。
        /// </summary>
        public JsonSerializeOption Option { get; }
    }
}
#endif