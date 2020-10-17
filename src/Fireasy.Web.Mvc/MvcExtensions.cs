// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System.Linq;
#if !NETCOREAPP
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace Fireasy.Web.Mvc
{
    public static class MvcExtensions
    {
        /// <summary>
        /// 包装 <see cref="JsonResult"/>，以便能够使用 <see cref="JsonConverter"/> 来转换需要的数据格式。
        /// </summary>
        /// <param name="controller">控制器对象。</param>
        /// <param name="value">输出的对象值。</param>
        /// <param name="converters">一组 Json 序列化转换器对象。</param>
        /// <returns></returns>
        public static JsonResult Json(this Controller controller, object value, params JsonConverter[] converters)
        {
            var option = new JsonSerializeOption();

            if (converters != null)
            {
                option.Converters.AddRange(converters.Where(s => s != null));
            }

            return new JsonResultWrapper(value, option);
        }
    }
}
