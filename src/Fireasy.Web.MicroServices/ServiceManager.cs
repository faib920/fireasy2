// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Logging;
using Fireasy.Common.Serialization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fireasy.Web.MicroServices
{
    /// <summary>
    /// 服务管理类。
    /// </summary>
    internal class ServiceManager
    {
        private static SafetyDictionary<string, HandlerDefinition> definitions = new SafetyDictionary<string, HandlerDefinition>();
        private static SafetyDictionary<string, List<string>> services = new SafetyDictionary<string, List<string>>();

        /// <summary>
        /// 从程序集里添加请求处理类。
        /// </summary>
        /// <param name="assembly"></param>
        internal static void AddHandlers(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes().Where(s => typeof(IRequestHandler).IsAssignableFrom(s)))
            {
                var method = type.GetMethod(nameof(IRequestHandler.ProcessAsync));
                var requestType = method.GetParameters()[0].ParameterType;
                var responseType = method.ReturnType;
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    responseType = responseType.GetGenericArguments()[0];
                }

                if (requestType == null || responseType == null)
                {
                    continue;
                }

                var attr = requestType.GetCustomAttribute<RequestTicketAttribute>();
                var name = attr != null ? attr.Name : type.FullName;

                definitions.TryAdd(name, () =>
                    {
                        return new HandlerDefinition { RequestType = requestType, ResponseType = responseType, Handler = type.New<IRequestHandler>() };
                    });
            }
        }

        /// <summary>
        /// 返回是否授权通过。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        internal static bool IsAuthenticated(HttpContext context, MicroServiceOption option)
        {
            if ((option.UseAuthentication && !context.User.Identity.IsAuthenticated) ||
                (!string.IsNullOrEmpty(option.IpWhiteAddress) && !ValidateIpAddress(GetIpAddress(context), option.IpWhiteAddress)))
            {
                return false;
            }

            return true;
        }

        internal static async Task ExecuteAsync(HttpContext context, MicroServiceOption option)
        {
            //发现服务
            if (context.Request.Path.Value.EndsWith("/discover"))
            {
                await ProcessDiscoverAsync(context, option);
            }
            //健康检查
            else if (context.Request.Path.Value.EndsWith("/health_check"))
            {
                context.Response.StatusCode = 200;
            }
            //执行请求
            else if (context.Request.Path.Value.EndsWith("/execute"))
            {
                await ProcessExecuteAsync(context);
            }
        }

        private static async Task ProcessDiscoverAsync(HttpContext context, MicroServiceOption option)
        {
            var url = context.Request.Scheme + "://" + context.Request.Host.Value + option.Path;
            var handlers = definitions.Select(s => s.Key).ToArray();
            var response = new ResponseBase<object> { Succeed = true, Data = new { url, handlers } };
            await WriteResponseAsync(context, response);
        }

        private static async Task ProcessExecuteAsync(HttpContext context)
        {
            IClientResponse response;
            if (!context.Request.Query.ContainsKey("name"))
            {
                response = new ResponseBase<object> { ErrorMessage = $"缺少 name 参数。" };
            }
            else
            {
                var name = context.Request.Query["name"];
                var definition = GetHandler(name);

                if (definition == null)
                {
                    response = new ResponseBase { ErrorMessage = $"没有发现服务标识 {name}。" };
                }
                else
                {
                    try
                    {
                        var request = ParseRequest(context, definition.RequestType);
                        response = await definition.Handler.ProcessAsync(request);
                    }
                    catch (ClientNotificationException exp)
                    {
                        response = new ResponseBase { ErrorMessage = exp.Message };
                    }
                    catch (Exception exp)
                    {
                        response = new ResponseBase { ErrorMessage = "发生错误，请查看服务日志。" };
                        var logger = LoggerFactory.CreateLogger();
                        logger?.Error($"执行 {name} 时发生异常。", exp);
                    }
                }
            }

            await WriteResponseAsync(context, response);
        }

        private static IClientRequst ParseRequest(HttpContext context, Type requestType)
        {
            var serializer = new JsonSerializer();

            using (var reader = new StreamReader(context.Request.Body))
            {
                var content = reader.ReadToEnd();
                return (IClientRequst)serializer.Deserialize(content, requestType);
            }
        }

        private static async Task WriteResponseAsync(HttpContext context, IClientResponse response)
        {
            var serializer = new JsonSerializer();
            var content = serializer.Serialize(response);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(content);
        }

        private static HandlerDefinition GetHandler(string name)
        {
            if (definitions.TryGetValue(name, out HandlerDefinition definition))
            {
                return definition;
            }

            return null;
        }

        private static string GetIpAddress(HttpContext context)
        {
            return GetIpAddress(context.Connection.RemoteIpAddress, context.Connection.RemotePort);
        }

        private static string GetIpAddress(IPAddress ipAddress, int port)
        {
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }

            return new IPEndPoint(ipAddress, port).ToString();
        }

        private static bool ValidateIpAddress(string ip, string ipWhiteAddress)
        {
            return ipWhiteAddress.Split(';').Any(s => Regex.IsMatch(ip, s));
        }
    }

    internal class HandlerDefinition
    {
        internal IRequestHandler Handler { get; set; }

        internal Type RequestType { get; set; }

        internal Type ResponseType { get; set; }
    }
}
