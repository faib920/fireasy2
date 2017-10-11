// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 数据库存储过程参数信息。
    /// </summary>
    public class ProcedureParameter : ISchemaMetadata
    {
        /// <summary>
        /// 获取分录名称。
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// 获取架构名称。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 获取存储过程名称。
        /// </summary>
        public string ProcedureName { get; set; }

        /// <summary>
        /// 获取参数名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取参数类型。
        /// </summary>
        public ParameterDirection Direction { get; set; }

        /// <summary>
        /// 获取参数的数据类型。
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 获取参数的小数精度。
        /// </summary>
        public int NumericPrecision { get; set; }

        /// <summary>
        /// 获取参数的小数范围。
        /// </summary>
        public int NumericScale { get; set; }

        /// <summary>
        /// 获取参数的长度。
        /// </summary>
        public long Length { get; set; }
    }
}
