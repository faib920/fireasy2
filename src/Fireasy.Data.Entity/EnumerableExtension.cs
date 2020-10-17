// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体集的一些扩展方法。
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// 将一个实体序列转换为实体集。
        /// </summary>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <param name="source">一个包含实体对象的序列。</param>
        /// <returns></returns>
        public static EntitySet<TEntity> ToEntitySet<TEntity>(this IEnumerable<TEntity> source) where TEntity : class, IEntity
        {
            return new EntitySet<TEntity>(source);
        }

        /// <summary>
        /// 将一个实体序列转换为 <see cref="DataTable"/> 对象。
        /// </summary>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <param name="source">一个包含实体对象的序列。</param>
        /// <param name="tableName">指定 <see cref="DataTable"/> 的表名，如果忽略该参数，则根据实体的类型进行获取。</param>
        /// <param name="changedOnly">仅仅转换状态改变的实体。</param>
        /// <returns></returns>
        public static DataTable ToDataTable<TEntity>(this IEnumerable<TEntity> source, string tableName = null, bool changedOnly = false) where TEntity : IEntity
        {
            if (source.IsNullOrEmpty())
            {
                return null;
            }
            var entityType = source.FirstOrDefault().GetType();
            var properties = new List<IProperty>(PropertyUnity.GetPersistentProperties(entityType));

            if (string.IsNullOrEmpty(tableName))
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
                tableName = metadata.TableName;
            }

            var table = new DataTable(tableName);
            var keys = CreateTableColumns(table, properties);

            if (changedOnly)
            {
                FillDataTableRows(table, source, properties, keys.Length > 0);
            }
            else
            {
                FillDataTableRows(table, source, properties, keys.Length > 0);
            }
            if (keys.Length > 0)
            {
                var con = new UniqueConstraint(keys, true);
                table.Constraints.Add(con);
            }
            return table;
        }

        /// <summary>
        /// 根据实体的属性建立 DataTable 的列。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="properties"></param>
        /// <remarks>主键的数组。</remarks>
        private static DataColumn[] CreateTableColumns(DataTable table, IEnumerable<IProperty> properties)
        {
            var primaryColumns = new List<DataColumn>();
            foreach (var property in properties)
            {
                var column = property.ToDataColumn();
                if (column == null)
                {
                    continue;
                }
                table.Columns.Add(column);
                //主键
                if (property.Info.IsPrimaryKey)
                {
                    primaryColumns.Add(column);
                }
                //自增长
                if (property.Info.GenerateType == IdentityGenerateType.AutoIncrement)
                {
                    column.AutoIncrement = true;
                }
            }
            return primaryColumns.ToArray();
        }

        private static void FillDataTableRows(DataTable table, IEntitySet extendList, IList<IProperty> properties, bool hasPrmKey)
        {
            FillDataTableRows(table, extendList.GetAttachedList(), properties, hasPrmKey, EntityState.Attached);
            FillDataTableRows(table, extendList.GetModifiedList(), properties, hasPrmKey, EntityState.Modified);
            FillDataTableRows(table, extendList.GetDetachedList(), properties, hasPrmKey, EntityState.Detached);
            extendList.Reset();
        }

        /// <summary>
        /// 填充 DataTable 的数据行。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="source"></param>
        /// <param name="properties"></param>
        /// <param name="hasPrmKey"></param>
        /// <param name="state"></param>
        private static void FillDataTableRows(DataTable table, IEnumerable source, IList<IProperty> properties, bool hasPrmKey, EntityState? state = null)
        {
            var count = properties.Count;

            var enumer = source.GetEnumerator();
            while (enumer.MoveNext())
            {
                var values = new object[count];
                var entity = enumer.Current.As<IEntity>();
                for (var i = 0; i < count; i++)
                {
                    values[i] = ReadEntityValue(entity, properties[i], hasPrmKey);
                }

                var row = table.LoadDataRow(values, LoadOption.PreserveChanges);
                ModifyRowState(state ?? entity.EntityState, row, hasPrmKey);

                if (!hasPrmKey && (state ?? entity.EntityState) == EntityState.Modified)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var value = entity.GetValue(properties[i]);
                        row[i] = PropertyValue.IsEmpty(value) ? DBNull.Value : value.GetValue();
                    }
                }
            }
        }

        private static object ReadEntityValue(IEntity entity, IProperty property, bool hasPrmKey)
        {
            //如果没有主键，则先读取属性修改前的值
            if (!hasPrmKey)
            {
                var d1 = entity.GetOldValue(property);
                if (d1 != null)
                {
                    return d1.GetValue();
                }
            }

            var d = entity.GetValue(property);
            if (PropertyValue.IsEmpty(d))
            {
                d = entity.GetOldValue(property);
            }

            if (!PropertyValue.IsEmpty(d))
            {
                return d.GetValue();
            }

            if (!PropertyValue.IsEmpty(property.Info.DefaultValue))
            {
                return property.Info.DefaultValue.GetValue();
            }

            return null;
        }

        private static void ModifyRowState(EntityState state, DataRow row, bool hasPrmKey)
        {
            switch (state)
            {
                case EntityState.Attached:
                    row.SetAdded();
                    break;
                case EntityState.Modified:
                    if (hasPrmKey)
                    {
                        row.SetModified();
                    }
                    break;
                case EntityState.Detached:
                    row.Delete();
                    break;
            }
        }
    }
}
