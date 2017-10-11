// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using System;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 返回结果的结构定义。
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 获取或设置是否调用成功。
        /// </summary>
        [TextSerializeElement("succeed")]
        public bool Succeed { get; set; }

        /// <summary>
        /// 获取或设置调用的信息。
        /// </summary>
        [TextSerializeElement("msg")]
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置客户端接收的数据。
        /// </summary>
        [TextSerializeElement("data")]
        public object Data { get; set; }

        /// <summary>
        /// 返回一个提醒的响应信息。
        /// </summary>
        /// <param name="message">提供给客户端显示的信息。</param>
        /// <param name="data">返回给客户端的数据。</param>
        /// <returns></returns>
        public static Result Info(string message, object data = null)
        {
            return new Result { Succeed = true, Message = message, Data = data };
        }

        /// <summary>
        /// 返回一个提醒的响应信息。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="message">提供给客户端显示的信息。</param>
        /// <param name="data">返回给客户端的数据。</param>
        /// <returns></returns>
        public static Result<T> Info<T>(string message, T data = default(T))
        {
            return new Result<T> { Succeed = true, Message = message, Data = data };
        }

        /// <summary>
        /// 返回一个成功的响应信息。
        /// </summary>
        /// <param name="message">提供给客户端显示的调用成功的信息。</param>
        /// <param name="data">返回给客户端的数据。</param>
        /// <returns></returns>
        public static Result Success(string message, object data = null)
        {
            return new Result { Succeed = true, Message = message, Data = data };
        }

        /// <summary>
        /// 返回一个成功的响应信息。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="message">提供给客户端显示的调用成功的信息。</param>
        /// <param name="data">返回给客户端的数据。</param>
        /// <returns></returns>
        public static Result<T> Success<T>(string message, T data = default(T))
        {
            return new Result<T> { Succeed = true, Message = message, Data = data };
        }

        /// <summary>
        /// 返回一个成功的响应信息。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="message">提供给客户端显示的调用成功的信息。</param>
        /// <param name="data">返回给客户端的数据。</param>
        /// <returns></returns>
        public static Result<T> Success<T>(string message, object data = null)
        {
            T value = default(T);
            if (data != null)
            {
                value = data.To<T>();
            }

            return new Result<T> { Succeed = true, Message = message, Data = value };
        }

        /// <summary>
        /// 返回一个失败的响应信息。
        /// </summary>
        /// <param name="message">提供给客户端显示的调用失败的信息。</param>
        /// <returns></returns>
        public static Result Fail(string message)
        {
            return new Result { Succeed = false, Message = message };
        }

        /// <summary>
        /// 返回一个失败的响应信息。
        /// </summary>
        /// <param name="message">提供给客户端显示的调用失败的信息。</param>
        /// <returns></returns>
        public static Result<T> Fail<T>(string message)
        {
            return new Result<T> { Succeed = false, Message = message };
        }

        /// <summary>
        /// 返回一个失败的响应信息。
        /// </summary>
        /// <param name="exception">引发调用失败的异常信息。</param>
        /// <returns></returns>
        public static Result Fail(Exception exception)
        {
            return new Result { Succeed = false, Message = exception.Message };
        }

        /// <summary>
        /// 返回一个失败的响应信息。
        /// </summary>
        /// <param name="exception">引发调用失败的异常信息。</param>
        /// <returns></returns>
        public static Result<T> Fail<T>(Exception exception)
        {
            return new Result<T> { Succeed = false, Message = exception.Message };
        }
    }

    /// <summary>
    /// 返回结果的结构定义，采用泛型。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// 获取或设置是否调用成功。
        /// </summary>
        [TextSerializeElement("succeed")]
        public bool Succeed { get; set; }

        /// <summary>
        /// 获取或设置调用的信息。
        /// </summary>
        [TextSerializeElement("msg")]
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置客户端接收的数据。
        /// </summary>
        [TextSerializeElement("data")]
        public new T Data { get; set; }

        public static implicit operator Result<T>(T data)
        {
            return new Result<T> { Succeed = true, Data = data };
        }
    }
}
