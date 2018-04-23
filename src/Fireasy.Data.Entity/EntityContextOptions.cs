// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data.Entity
{
    public class EntityContextOptions
    {
        /// <summary>
        /// 获取或设置是否自动创建数据表。默认为 false。
        /// </summary>
        public bool AutoCreateTables { get; set; }

        /// <summary>
        /// 获取或设置配置名称。
        /// </summary>
        public string ConfigName { get; set; }
    }
}
