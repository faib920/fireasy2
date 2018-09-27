// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 调用的消息包。
    /// </summary>
    public class InvokeMessage
    {
        /// <summary>
        /// 初始化 <see cref="InvokeMessage"/> 类的新实例。
        /// </summary>
        public InvokeMessage()
        {
        }

        /// <summary>
        /// 初始化 <see cref="InvokeMessage"/> 类的新实例。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="direction"></param>
        /// <param name="arguments"></param>
        internal InvokeMessage(string method, int direction, object[] arguments)
        {
            Method = method;
            Direction = direction;
            Arguments = arguments;
        }

        /// <summary>
        /// 获取或设置方法名称。
        /// </summary>
        [TextSerializeElement("M")]
        public string Method { get; set; }

        /// <summary>
        /// 获取或设置消息方向。0: 调用, 1: 返回。
        /// </summary>
        [TextSerializeElement("D")]
        public int Direction { get; set; }

        /// <summary>
        /// 获取或设置调用参数/返回值。
        /// </summary>
        [TextSerializeElement("A")]
        public object[] Arguments { get; set; }

        /// <summary>
        /// 获取或设置是否有返回值。
        /// </summary>
        [TextSerializeElement("R")]
        public int IsReturn { get; set; }
    }
}
