// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Web.MicroServices
{
    /// <summary>
    /// 用于在 <see cref="RequestBase{TResult}"/> 类上标记请求处理程序的唯一标识。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequestTicketAttribute : Attribute
    {
        public RequestTicketAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
