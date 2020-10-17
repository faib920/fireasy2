// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data
{
    /// <summary>
    /// 表示一个数据库连接的参数。无法继承此类。
    /// </summary>
    public sealed class ConnectionParameter
    {
        /// <summary>
        /// 获取或设置服务器。
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 获取或设置数据库名。
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 获取或设置用户名。
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 获取或设置用户密码。
        /// </summary>
        public string Password { get; set; }
    }
}
