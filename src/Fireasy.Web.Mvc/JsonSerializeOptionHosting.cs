// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Fireasy.Common.Serialization;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 用于在 Action 执行期间附加 <see cref="JsonSerializeOption"/> 参数设置。
    /// </summary>
    public class JsonSerializeOptionHosting
    {
        /// <summary>
        /// 获取 <see cref="JsonSerializeOption"/> 对象。
        /// </summary>
        public JsonSerializeOption Option { get; } = new JsonSerializeOption();
    }
}
#endif