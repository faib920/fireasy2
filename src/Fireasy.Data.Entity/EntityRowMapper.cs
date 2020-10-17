// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 一个用于将数据行转换为实体的映射器。无法继承此类。
    /// </summary>
    /// <typeparam name="T">要转换的实体类型。</typeparam>
    public sealed class EntityRowMapper<T> : FieldRowMapperBase<T> where T : class
    {
        private IEnumerable<PropertyMapping> _mapping;
        private IEnumerable<IProperty> _alwaysProperties;

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public override T Map(IDatabase database, IDataReader reader)
        {
            if (_mapping == null)
            {
                InitMapping(GetDataReaderFields(reader));
            }

            return MapInternal(mapper => GetPropertyValue(mapper, reader.IsDBNull, reader.GetValue, isNull => GetSupportedValue(isNull, mapper, reader)));
        }

        private T MapInternal(Func<PropertyMapping, PropertyValue> func)
        {
            var proxyType = EntityProxyManager.GetType((Type)null, typeof(T));
            var entity = proxyType.New<IEntity>();
            if (entity == null)
            {
                return default;
            }

            entity.SetState(EntityState.Unchanged);
            entity.As<ISupportInitializeNotification>(s => s.BeginInit());

            foreach (var mapper in _mapping)
            {
                var value = func(mapper);
                entity.InitializeValue(mapper.Property, value);
            }

            if (_alwaysProperties != null)
            {
                foreach (var property in _alwaysProperties)
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
                return typeCode switch
                {
                    TypeCode.Boolean => isNull ? new bool?() : RecordWrapper.GetBoolean(reader, mapper.Index),
                    TypeCode.Char => isNull ? new char?() : RecordWrapper.GetChar(reader, mapper.Index),
                    TypeCode.Byte => isNull ? new byte?() : RecordWrapper.GetByte(reader, mapper.Index),
                    TypeCode.Int16 => isNull ? new short?() : RecordWrapper.GetInt16(reader, mapper.Index),
                    TypeCode.Int32 => isNull ? new int?() : RecordWrapper.GetInt32(reader, mapper.Index),
                    TypeCode.Int64 => isNull ? new long?() : RecordWrapper.GetInt64(reader, mapper.Index),
                    TypeCode.Decimal => isNull ? new decimal?() : RecordWrapper.GetDecimal(reader, mapper.Index),
                    TypeCode.Double => isNull ? new double?() : RecordWrapper.GetDouble(reader, mapper.Index),
                    TypeCode.String => isNull ? string.Empty : RecordWrapper.GetString(reader, mapper.Index),
                    TypeCode.DateTime => isNull ? new DateTime?() : RecordWrapper.GetDateTime(reader, mapper.Index),
                    TypeCode.Single => isNull ? new float?() : RecordWrapper.GetFloat(reader, mapper.Index),
                    _ => PropertyValue.NewValue(RecordWrapper.GetValue(reader, mapper.Index)),
                };
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
            return typeCode switch
            {
                TypeCode.Boolean => isNull ? new bool?() : Convert.ToBoolean(row[mapper.Index]),
                TypeCode.Char => isNull ? new char?() : Convert.ToChar(row[mapper.Index]),
                TypeCode.Byte => isNull ? new byte?() : Convert.ToByte(row[mapper.Index]),
                TypeCode.Int16 => isNull ? new short?() : Convert.ToInt16(row[mapper.Index]),
                TypeCode.Int32 => isNull ? new int?() : Convert.ToInt32(row[mapper.Index]),
                TypeCode.Int64 => isNull ? new long?() : Convert.ToInt64(row[mapper.Index]),
                TypeCode.Decimal => isNull ? new decimal?() : Convert.ToDecimal(row[mapper.Index]),
                TypeCode.Double => isNull ? new double?() : Convert.ToDouble(row[mapper.Index]),
                TypeCode.String => isNull ? string.Empty : Convert.ToString(row[mapper.Index]),
                TypeCode.DateTime => isNull ? new DateTime?() : Convert.ToDateTime(row[mapper.Index]),
                TypeCode.Single => isNull ? new float?() : Convert.ToSingle(row[mapper.Index]),
                _ => PropertyValue.NewValue(row[mapper.Index], null),
            };
        }

        private void InitMapping(string[] fields)
        {
            _mapping = from s in PropertyUnity.GetProperties(typeof(T))
                       let index = IndexOf(fields, s)
                       where index != -1 && s is ILoadedProperty
                       select new PropertyMapping { Property = s, Index = index };
            _alwaysProperties = PropertyUnity.GetRelatedProperties(typeof(T), LoadBehavior.Always);
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

                if (property.Info.ColumnName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
