// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common;
using Fireasy.Common.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 控制器操作方法执行期间的上下文对象。
    /// </summary>
    public class ActionContext : Scope<ActionContext>
    {
        internal ActionContext(ControllerContext controllerContext)
        {
            ControllerContext = controllerContext;
            Converters = new List<ITextConverter>();
        }

        /// <summary>
        /// 获取控制器上下文对象。
        /// </summary>
        public ControllerContext ControllerContext { get; private set; }

        /// <summary>
        /// 获取动作相关的 <see cref="ActionDescriptor"/> 对象。
        /// </summary>
        public ActionDescriptor ActionDescriptor { get; internal set; }

        /// <summary>
        /// 获取文本转换器列表。
        /// </summary>
        public List<ITextConverter> Converters { get; private set; }

        /// <summary>
        /// 获取动作方法的参数字典。
        /// </summary>
        public IDictionary<string, object> Parameters { get; internal set; }
    }
}
#endif