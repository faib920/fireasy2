// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 拦截器触发的事件类型。
    /// </summary>
    public enum InterceptType
    {
        /// <summary>
        /// 无。
        /// </summary>
        None,

        /// <summary>
        /// 在调用方法之前。
        /// </summary>
        BeforeMethodCall,

        /// <summary>
        /// 在调用方法之后。
        /// </summary>
        AfterMethodCall,

        /// <summary>
        /// 在获取属性之前。
        /// </summary>
        BeforeGetValue,

        /// <summary>
        /// 在获取属性之后。
        /// </summary>
        AfterGetValue,

        /// <summary>
        /// 在设置属性之前。
        /// </summary>
        BeforeSetValue,

        /// <summary>
        /// 在设置属性之后。
        /// </summary>
        AfterSetValue,

        /// <summary>
        /// 异常处理的 catch 块。
        /// </summary>
        Catching,

        /// <summary>
        /// 异常处理的 finaly 块。
        /// </summary>
        Finally
    }
}
