// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 一个用于将数据行转换为实体的映射器。无法继承此类。
    /// </summary>
    /// <typeparam name="T">要转换的实体类型。</typeparam>
    public sealed class EntityRowMapper<T> : FieldRowMapperBase<T> where T : class
    {
        private IEnumerable<PropertyMapping> mapping;
        private IEnumerable<IProperty> alwaysProperties;

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public override T Map(IDataReader reader)
        {
            if (mapping == null)
            {
                InitMapping(GetDataReaderFields(reader));
            }

            return MapInternal(mapper => GetPropertyValue(mapper, reader.IsDBNull, reader.GetValue, isNull => GetSupportedValue(isNull, mapper, reader)));
        }

        /// <summary>
        /// 将一个 <see cref="DataRow"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="row">一个 <see cref="DataRow"/> 对象。</param>
        /// <returns>由 <see cref="DataRow"/> 中数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public override T Map(DataRow row)
        {
            if (mapping == null)
            {
                InitMapping(GetDataRowFields(row));
            }

            return MapInternal(mapper => GetPropertyValue(mapper, index => row[index] == DBNull.Value, index => row[index], isNull => GetSupportedValue(isNull, mapper, row)));
        }

        private T MapInternal(Func<PropertyMapping, PropertyValue> func)
        {
            var proxyType = EntityProxyManager.GetType(typeof(T));
            var entity = proxyType.New<IEntity>();
            if (entity == null)
            {
                return default(T);
            }

            if (Initializer != null)
            {
                Initializer(entity);
            }

            entity.SetState(EntityState.Unchanged);
            entity.As<ISupportInitializeNotification>(s => s.BeginInit());

            foreach (var mapper in mapping)
            {
                var value = func(mapper);
                entity.InitializeValue(mapper.Property, value);
            }

            if (alwaysProperties != null)
            {
                foreach (var property in alwaysProperties)
                {
                    EntityLazyloader.AsyncLoad(entity, property);
                }
            }

            entity.As<ISupportInitializeNotification>(s => s.EndInit());
            return (T)entity;
        }

        private PropertyValue GetPropertyValue(PropertyMapping mapper, Func<int, bool> funcIsNull, Func<int, object> funcGetValue, Func<bool, PropertyValue> funcRead)
        {
            var isNull = funcIsNull(mapper.Index);
            if (mapper.Property.Type.IsArray &&
                mapper.Property.Type.GetElementType() == typeof(byte))
            {
                return isNull ? new byte[0] : (byte[])funcGetValue(mapper.Index);
            }

            if (mapper.Property.Type.IsEnum)
            {
                return GetEnumValue(mapper, isNull, funcGetValue);
            }

            if (PropertyValue.IsSupportedType(mapper.Property.Type))
            {
                return funcRead(isNull);
            }

            return GetConvertedValue(mapper, isNull, funcGetValue);
        }

        private PropertyValue GetEnumValue(PropertyMapping mapper, bool isNull, Func<int, object> funcGetValue)
        {
            if (isNull)
            {
                return mapper.Property.Type.New<Enum>();
            }

            return (Enum)Enum.Parse(mapper.Property.Type, funcGetValue(mapper.Index).ToString());
        }

        private PropertyValue GetConvertedValue(PropertyMapping mapper, bool isNull, Func<int, object> funcGetValue)
        {
            var converter = ConvertManager.GetConverter(mapper.Property.Type);
            if (converter == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableConvertComplexType, mapper.Property.Type));
            }

            if (isNull)
            {
                return PropertyValue.Empty;
            }

            var dbType = mapper.Property.Info.DataType ?? DbType.String;
            return PropertyValue.NewValue(converter.ConvertFrom(funcGetValue(mapper.Index), dbType));
        }

        private PropertyValue GetSupportedValue(bool isNull, PropertyMapping mapper, IDataReader reader)
        {
            try
            {
                if (mapper.Property.Type == typeof(Guid))
                {
                    return isNull ? new Guid?() : new Guid(reader.GetValue(mapper.Index).ToString());
                }

                var typeCode = Type.GetTypeCode(mapper.Property.Type.GetNonNullableType());
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return isNull ? new bool?() : RecordWrapper.GetBoolean(reader, mapper.Index);
                    case TypeCode.Char:
                        return isNull ? new char?() : RecordWrapper.GetChar(reader, mapper.Index);
                    case TypeCode.Byte:
                        return isNull ? new byte?() : RecordWrapper.GetByte(reader, mapper.Index);
                    case TypeCode.Int16:
                        return isNull ? new short?() : RecordWrapper.GetInt16(reader, mapper.Index);
                    case TypeCode.Int32:
                        return isNull ? new int?() : RecordWrapper.GetInt32(reader, mapper.Index);
                    case TypeCode.Int64:
                        return isNull ? new long?() : RecordWrapper.GetInt64(reader, mapper.Index);
                    case TypeCode.Decimal:
                        return isNull ? new decimal?() : RecordWrapper.GetDecimal(reader, mapper.Index);
                    case TypeCode.Double:
                        return isNull ? new double?() : RecordWrapper.GetDouble(reader, mapper.Index);
                    case TypeCode.String:
                        return isNull ? string.Empty : RecordWrapper.GetString(reader, mapper.Index);
                    case TypeCode.DateTime:
                        return isNull ? new DateTime?() : RecordWrapper.GetDateTime(reader, mapper.Index);
                    case TypeCode.Single:
                        return isNull ? new float?() : RecordWrapper.GetFloat(reader, mapper.Index);
                    default:
                        return PropertyValue.NewValue(RecordWrapper.GetValue(reader, mapper.Index));
                }
            }
            catch (Exception ex)
            {
                throw new RowMapperCastException(mapper.Property.Name, mapper.Property.Type, ex);
            }
        }

        private PropertyValue GetSupportedValue(bool isNull, PropertyMapping mapper, DataRow row)
        {
            if (mapper.Property.Type == typeof(Guid))
            {
                return isNull ? new Guid?() : new Guid(row[mapper.Index].ToString());
            }

            var typeCode = Type.GetTypeCode(mapper.Property.Type.GetNonNullableType());
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return isNull ? new bool?() : Convert.ToBoolean(row[mapper.Index]);
                case TypeCode.Char:
                    return isNull ? new char?() : Convert.ToChar(row[mapper.Index]);
                case TypeCode.Byte:
                    return isNull ? new byte?() : Convert.ToByte(row[mapper.Index]);
                case TypeCode.Int16:
                    return isNull ? new short?() : Convert.ToInt16(row[mapper.Index]);
                case TypeCode.Int32:
                    return isNull ? new int?() : Convert.ToInt32(row[mapper.Index]);
                case TypeCode.Int64:
                    return isNull ? new long?() : Convert.ToInt64(row[mapper.Index]);
                case TypeCode.Decimal:
                    return isNull ? new decimal?() : Convert.ToDecimal(row[mapper.Index]);
                case TypeCode.Double:
                    return isNull ? new double?() : Convert.ToDouble(row[mapper.Index]);
                case TypeCode.String:
                    return isNull ? string.Empty : Convert.ToString(row[mapper.Index]);
                case TypeCode.DateTime:
                    return isNull ? new DateTime?() : Convert.ToDateTime(row[mapper.Index]);
                case TypeCode.Single:
                    return isNull ? new float?() : Convert.ToSingle(row[mapper.Index]);
                default:
                    return PropertyValue.NewValue(row[mapper.Index], null);
            }
        }

        private void InitMapping(string[] fields)
        {
            mapping = from s in PropertyUnity.GetProperties(typeof(T))
                      let index = IndexOf(fields, s)
                      where index != -1 && s is ILoadedProperty
                      select new PropertyMapping { Property = s, Index = index };
            alwaysProperties = PropertyUnity.GetRelatedProperties(typeof(T), LoadBehavior.Always);
        }

        private int IndexOf(string[] fields, IProperty property)
        {
            for (var i = 0; i < fields.Length; i++)
            {
                var fieldName = fields[i];
                var p = fieldName.IndexOf('.');
                if (p != -1)
                {
                    fieldName = fieldName.Substring(p + 1);
                }

                if (property.Info.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
