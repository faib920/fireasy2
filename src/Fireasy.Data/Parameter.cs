// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Extensions;
using System;
using System.Data;
using System.Diagnostics;

namespace Fireasy.Data
{
    /// <summary>
    /// 查询参数。无法继承此类。
    /// </summary>
    [DebuggerDisplay("Name={ParameterName},DbType={DbType},Value={Value}")]
    [Serializable]
    public class Parameter : ICloneable
    {
        private object _parValue;

        /// <summary>
        /// 初始化 <see cref="Parameter"/> 类的新实例。
        /// </summary>
        private Parameter()
        {
            SourceVersion = DataRowVersion.Current;
        }

        /// <summary>
        /// 初始化 <see cref="Parameter"/> 类的新实例。
        /// </summary>
        /// <param name="parameterName">参数名。</param>
        public Parameter(string parameterName)
            : this()
        {
            ParameterName = parameterName;
            Direction = ParameterDirection.Input;
        }

        /// <summary>
        /// 初始化 <see cref="Parameter"/> 类的新实例。
        /// </summary>
        /// <param name="parameterName">参数名。</param>
        /// <param name="value">参数值。</param>
        public Parameter(string parameterName, object value)
        {
            ParameterName = parameterName;
            Value = value;
            Direction = ParameterDirection.Input;
        }

        /// <summary>
        /// 获取或设置参数名称。
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// 获取或设置参数的方向。
        /// </summary>
        public ParameterDirection Direction { get; set; }

        /// <summary>
        /// 获取或设置参数的值。
        /// </summary>
        public object Value
        {
            get
            {
                return _parValue;
            }

            set
            {
                _parValue = value;
                DbType dbType;
                if (value != null &&
                    value != DBNull.Value &&
                    DbType != (dbType = value.GetType().GetDbType()))
                {
                    DbType = dbType;
                }
            }
        }

        /// <summary>
        /// 获取或设置参数的数据类型。
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        ///  获取或设置该值指示参数是否接受空值。
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// 获取或设置源列的名称。
        /// </summary>
        public string SourceColumn { get; set; }

        /// <summary>
        /// 获取或设置加载 Value 时使用的 <see cref="DataRowVersion"></see>。
        /// </summary>
        public DataRowVersion SourceVersion { get; set; }

        /// <summary>
        /// 获取或设置alue 属性的最大位数。
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        /// 获取或设置Value的小数位数。
        /// </summary>
        public byte Scale { get; set; }

        /// <summary>
        /// 获取或设置参数的数据长度。
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 将参数复制一个副本。
        /// </summary>
        /// <returns>一个 <see cref="Parameter"/> 的浅表副本。</returns>
        public Parameter Clone()
        {
            return MemberwiseClone() as Parameter;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
