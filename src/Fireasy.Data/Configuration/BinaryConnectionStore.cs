// <copyright company="Faib Studio"
//      email="faib920@126.com"
//      qq="55570729"
//      date="2011-2-16">
//   (c) Copyright Faib Studio 2011. All rights reserved.
// </copyright>
// ---------------------------------------------------------------

using System;

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 二进制连接字符串存储结构，用于保存由 <see cref="BinaryInstanceSetting"/> 创建的连接字符串。
    /// </summary>
    [Serializable]
    public sealed class BinaryConnectionStore
    {
        /// <summary>
        /// 获取或设置数据提供者类型。
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 获取或设置数据库类型。
        /// </summary>
        public string DatabaseType { get; set; }

        /// <summary>
        /// 获取或设置数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }
    }

}
