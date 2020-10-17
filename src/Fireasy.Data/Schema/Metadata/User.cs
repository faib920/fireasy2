// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 数据库用户信息。
    /// </summary>
    public class User : ISchemaMetadata
    {
        /// <summary>
        /// 获取用户名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置创建时间。
        /// </summary>
        public DateTime? CreateDate { get; set; }
    }
}
