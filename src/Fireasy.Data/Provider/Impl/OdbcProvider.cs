// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD
using System;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// Odbc数据库提供者。
    /// </summary>
    public class OdbcProvider : ProviderBase
    {
        /// <summary>
        /// 提供 <see cref="OdbcProvider"/> 的静态实例。
        /// </summary>
        public readonly static OdbcProvider Instance = new OdbcProvider();

        /// <summary>
        /// 初始化 <see cref="OdbcProvider"/> 类的新实例。
        /// </summary>
        public OdbcProvider()
            : base(new InstallerProviderFactoryResolver("System.Data.Odbc"))
        {
        }

        public override string ProviderName => "Odbc";

        /// <summary>
        /// 获取当前连接的参数。
        /// </summary>
        /// <returns></returns>
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 使用参数更新指定的连接。
        /// </summary>
        /// <param name="connectionString">连接字符串对象。</param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override string UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}
#endif