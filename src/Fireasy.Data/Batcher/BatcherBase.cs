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
using Fireasy.Data.Converter;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// 数据批量插入的抽象类。
    /// </summary>
    public abstract class BatcherBase
    {
        /// <summary>
        /// 获取 <see cref="DataTable"/> 中字段名称和 DbType 的映射。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected IList<PropertyFieldMapping> GetNameTypeMapping(DataTable table)
        {
            var result = new List<PropertyFieldMapping>();

            table.EachColumn((c, i) =>
                {
                    result.Add(new PropertyFieldMapping(c.ColumnName, c.ColumnName, c.DataType, c.DataType.GetDbType())
                        {
                            ValueFunc = o => ((DataRow)o)[c.ColumnName]
                        });
                });

            return result;
        }
        
        /// <summary>
        /// 获取 List 中属性名和 DbType 的映射。
        /// </summary>
        /// <param name="item">元素。</param>
        /// <returns></returns>
        protected IList<PropertyFieldMapping> GetNameTypeMapping(object item)
        {
            var result = new List<PropertyFieldMapping>();
            TypeDescriptorUtility.AddDefaultDynamicProvider();
            var resolver = item as IPropertyFieldMappingResolver;
            if (resolver != null)
            {
                return resolver.GetDbMapping().ToList();
            }

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(item))
            {
                if (pd.PropertyType == null)
                {
                    throw new InvalidOperationException(SR.GetString(SRKind.PropertyTypeIsNull, item.GetType(), pd.Name));
                }

                result.Add(new PropertyFieldMapping(pd.Name, pd.Name, pd.PropertyType, pd.PropertyType.GetDbType())
                    {
                        ValueFunc = o => pd.GetValue(o)
                    });
            }

            return result;
        }

        /// <summary>
        /// 使用 <see cref="DataRow"/> 映射生成 Sql 语句。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="mapping"></param>
        /// <param name="row"></param>
        /// <param name="batch"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected string MapDataRow(IProvider provider, IList<PropertyFieldMapping> mapping, DataRow row, int batch, DbParameterCollection parameters)
        {
            var sb = new StringBuilder();
            var assert = new AssertFlag();
            var prefix = provider.GetService<ISyntaxProvider>().ParameterPrefix;

            var i = 0;
            foreach (var map in mapping)
            {
                if (!assert.AssertTrue())
                {
                    sb.Append(",");
                }

                var value = map.ValueFunc(row);
                if (value == DBNull.Value)
                {
                    sb.Append("NULL");
                }
                else
                {
                    sb.Append(BuildValueString(provider, map, parameters, value, i++, batch));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 使用 <see cref="IList"/> 映射生成 Sql 语句。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="mappings"></param>
        /// <param name="item"></param>
        /// <param name="batch"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected string MapListItem<T>(IProvider provider, IList<PropertyFieldMapping> mappings, T item, int batch, DbParameterCollection parameters)
        {
            var sb = new StringBuilder();
            var assert = new AssertFlag();

            for (var i = 0; i < mappings.Count; i++)
            {
                if (!assert.AssertTrue())
                {
                    sb.Append(",");
                }

                var value = mappings[i].ValueFunc(item);
                if (value == null)
                {
                    sb.Append("NULL");
                }
                else
                {
                    sb.Append(BuildValueString(provider, mappings[i], parameters, value, i, batch));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 根据 <paramref name="batchSize"/> 拆分集合中的数据。
        /// </summary>
        /// <param name="collection">数据集合。</param>
        /// <param name="batchSize">每一批次写入的数据大小。</param>
        /// <param name="rowAction">枚举第一条数据时执行的方法。参数1为记录索引，参数2为当前批次中记录的索引，参数3为当前的记录。</param>
        /// <param name="splitAction">数据达到一批时执行的方法。参数1为记录索引，参数2为当前批次中记录的索引，参数3为剩余的记录条数，参数4为是否为最后的批次。</param>
        protected void BatchSplitData(ICollection collection, int batchSize, Action<int, int, object> rowAction, Action<int, int, int, bool> splitAction)
        {
            int batch = 0, index = 0;
            var count = collection.Count;

            foreach (var item in collection)
            {
                rowAction(index, batch, item);

                var lastBatch = index == count - 1;

                if (++batch == batchSize || (batch != 0 && lastBatch))
                {
                    splitAction(index, batch, count - index - 1, lastBatch);
                    batch = 0;
                }

                index++;
            }
        }

        /// <summary>
        /// 转换为对象集合。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        protected ICollection ToCollection<T>(IEnumerable<T> source)
        {
            return source is ICollection ? (ICollection)source : source.ToList();
        }

        /// <summary>
        /// 判断是否需要放入到参数集合中。
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool NeedPutParameters(DbType dbType, object value)
        {
            switch(dbType)
            {
                case DbType.Binary:
                case DbType.Object:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Guid:
                    return true;
                case DbType.String:
                case DbType.AnsiString:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return value.ToStringSafely().IndexOf("'") != -1 || value.ToStringSafely().IndexOf("\\") != -1;
                default:
                    return false;
            }
        }

        private string AddOrReplayParameter(DbParameterCollection parameters, DbType dbType, ref object value, int index, int batch, Func<DbParameter> parFunc)
        {
            var name = string.Format("p_{0}_{1}", index, batch);

            if (parameters.Contains(name))
            {
                parameters[name].Value = value;
            }
            else
            {
                var parameter = parFunc();
                parameter.ParameterName = name;
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = value;
                parameters.Add(parameter);
            }

            return name;
        }

        private string BuildValueString(IProvider provider, PropertyFieldMapping mapping, DbParameterCollection parameters, object value, int index, int batch)
        {
            var sb = new StringBuilder();

            var converter = ConvertManager.GetConverter(mapping.PropertyType);
            if (converter != null)
            {
                value = converter.ConvertTo(value, mapping.FieldType);
            }

            if (value is Enum)
            {
                sb.Append((int)value);
            }
            else if (NeedPutParameters(mapping.FieldType, value))
            {
                var prefix = provider.GetService<ISyntaxProvider>().ParameterPrefix;
                var parameterName = AddOrReplayParameter(parameters, mapping.FieldType, ref value, index, batch,
                    () => provider.DbProviderFactory.CreateParameter());

                sb.AppendFormat("{0}{1}", prefix, parameterName);
            }
            else if (value is string || value is Guid)
            {
                sb.AppendFormat("'{0}'", value);
            }
            else
            {
                sb.Append(value);
            }

            return sb.ToString();
        }
    }
}
