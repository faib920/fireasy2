// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体属性的管理单元。
    /// </summary>
    public static class PropertyUnity
    {
        private const string NullFieldName = "NL<>";

        /// <summary>
        /// 注册基本的实体属性。
        /// </summary>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <param name="expression">指定注册的属性的表达式。</param>
        /// <param name="info">属性映射信息。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        public static IProperty RegisterProperty<TEntity>(Expression<Func<TEntity, object>> expression, PropertyMapInfo info = null) where TEntity : IEntity
        {
            var propertyInfo = PropertySearchVisitor.FindProperty(expression);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.InvalidRegisterExpression));
            }

            return RegisterProperty(propertyInfo, typeof(TEntity), info);
        }

        /// <summary>
        /// 注册基本的实体属性。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="propertyType">属性类型。</param>
        /// <param name="entityType">实体类型。</param>
        /// <param name="info">属性映射信息。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        public static IProperty RegisterProperty(string propertyName, Type propertyType, Type entityType, PropertyMapInfo info = null)
        {
            var propertyInfo = entityType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new PropertyNotFoundException(propertyName);
            }

            return RegisterProperty(propertyInfo, entityType, info);
        }

        /// <summary>
        /// 注册特殊的实体属性，这类属性为附加自实体间关系的不可持久化的属性。
        /// </summary>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <param name="expression">指定注册的属性的表达式。</param>
        /// <param name="referenceProperty">参数或引用的属性。</param>
        /// <param name="options">关联选项。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        public static IProperty RegisterSupposedProperty<TEntity>(Expression<Func<TEntity, object>> expression, IProperty referenceProperty = null, RelationOptions options = null) where TEntity : IEntity
        {
            var propertyInfo = PropertySearchVisitor.FindProperty(expression);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.InvalidRegisterExpression));
            }

            return RegisterSupposedProperty(propertyInfo, typeof(TEntity), referenceProperty, options);
        }

        /// <summary>
        /// 注册特殊的实体属性，这类属性为附加自实体间关系的不可持久化的属性。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="propertyType">属性类型。</param>
        /// <param name="entityType">实体类型。</param>
        /// <param name="referenceProperty">参数或引用的属性。</param>
        /// <param name="options">关联选项。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        public static IProperty RegisterSupposedProperty(string propertyName, Type propertyType, Type entityType, IProperty referenceProperty = null, RelationOptions options = null)
        {
            var propertyInfo = entityType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new PropertyNotFoundException(propertyName);
            }

            return RegisterSupposedProperty(propertyInfo, entityType, referenceProperty, options);
        }

        /// <summary>
        /// 注册实体属性，将属性放入到缓存表中。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="property">实体属性。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        public static IProperty RegisterProperty(Type entityType, IProperty property)
        {
            var metadata = EntityMetadataUnity.InternalGetEntityMetadata(entityType);
            if (metadata != null)
            {
                metadata.InternalAddProperty(InitProperty(entityType, property));
            }

            return property;
        }

        /// <summary>
        /// 获取指定名称的实体属性。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        public static IProperty GetProperty(Type entityType, string propertyName)
        {
            var metadata = EntityMetadataUnity.InternalGetEntityMetadata(entityType);
            if (metadata == null)
            {
                return null;
            }

            var properties = metadata.Filter(entityType);
            if (properties != null && properties.ContainsKey(propertyName))
            {
                return properties[propertyName];
            }

            return null;
        }

        /// <summary>
        /// 获取实体的所有属性。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象枚举器。</returns>
        public static IEnumerable<IProperty> GetProperties(Type entityType)
        {
            var metadata = EntityMetadataUnity.InternalGetEntityMetadata(entityType);
            if (metadata == null)
            {
                return Enumerable.Empty<IProperty>();
            }

            return metadata.Filter(entityType).Values;
        }

        /// <summary>
        /// 获取实体的具有主键的所有属性。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象枚举器。</returns>
        public static IEnumerable<IProperty> GetPrimaryProperties(Type entityType)
        {
            return GetProperties(entityType)?.Where(property => property is ISavedProperty && property.Info.IsPrimaryKey);
        }

        /// <summary>
        /// 获取实体的可持久化的属性。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象枚举器。</returns>
        public static IEnumerable<IProperty> GetPersistentProperties(Type entityType)
        {
            return GetProperties(entityType)?.Where(property => property is ISavedProperty);
        }

        /// <summary>
        /// 获取实体的可以加载的属性。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象枚举器。</returns>
        public static IEnumerable<IProperty> GetLoadedProperties(Type entityType)
        {
            return GetProperties(entityType)?.Where(property => property is ILoadedProperty);
        }

        /// <summary>
        /// 获取实体的关联属性。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="behavior">属性的加载行为。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象枚举器。</returns>
        public static IEnumerable<IProperty> GetRelatedProperties(Type entityType, LoadBehavior? behavior = null)
        {
            return from s in GetProperties(entityType)
                   let p = s as RelationProperty
                   where p != null
                   where behavior == null || (p.Options != null && p.Options.LoadBehavior == behavior)
                   select s;
        }

        /// <summary>
        /// 初始化实体类型的属性。
        /// </summary>
        /// <param name="entityType"></param>
        public static void Initialize(Type entityType)
        {
            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                //定义为 virtual
                var getMth = property.GetGetMethod();
                if (getMth != null && getMth.IsVirtual && !getMth.IsFinal)
                {
                    RegisterProperty(entityType, property);
                }
            }
        }

        /// <summary>
        /// 注册基本的实体属性。
        /// </summary>
        /// <param name="propertyInfo">属性信息。</param>
        /// <param name="entityType">实体类型。</param>
        /// <param name="info">属性映射信息。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        private static IProperty RegisterProperty(PropertyInfo propertyInfo, Type entityType, PropertyMapInfo info = null)
        {
            var property = new GeneralProperty
            {
                Name = propertyInfo.Name,
                Type = propertyInfo.PropertyType,
                EntityType = entityType,
                Info = InitPropertyInfo(info, propertyInfo)
            };

            return RegisterProperty(entityType, property);
        }

        /// <summary>
        /// 注册特殊的实体属性，这类属性为附加自实体间关系的不可持久化的属性。
        /// </summary>
        /// <param name="propertyInfo">属性名称。</param>
        /// <param name="entityType">实体类型。</param>
        /// <param name="referenceProperty">参数或引用的属性。</param>
        /// <param name="options">关联选项。</param>
        /// <returns>一个 <see cref="IProperty"/> 对象。</returns>
        private static IProperty RegisterSupposedProperty(PropertyInfo propertyInfo, Type entityType, IProperty referenceProperty = null, RelationOptions options = null)
        {
            IProperty property;
            var useAttr = propertyInfo.GetCustomAttributes<RelationshipUseAttribute>().FirstOrDefault();

            if (referenceProperty != null)
            {
                if (referenceProperty.Type.IsEnum)
                {
                    property = new EnumProperty
                    {
                        Name = propertyInfo.Name,
                        Type = propertyInfo.PropertyType,
                        EntityType = entityType,
                        RelationalType = referenceProperty.Type,
                        Reference = referenceProperty,
                        Info = InitRelatedPropertyInfo(propertyInfo),
                        Options = options ?? RelationOptions.Default
                    };
                }
                else
                {
                    //引用属性
                    property = new ReferenceProperty
                    {
                        Name = propertyInfo.Name,
                        Type = propertyInfo.PropertyType,
                        EntityType = entityType,
                        RelationalType = referenceProperty.EntityType,
                        Reference = referenceProperty,
                        Info = InitRelatedPropertyInfo(propertyInfo),
                        Options = options ?? RelationOptions.Default
                    };
                }
            }
            else if (typeof(IEntity).IsAssignableFrom(propertyInfo.PropertyType))
            {
                //实体引用属性
                property = new EntityProperty
                {
                    RelationalType = propertyInfo.PropertyType,
                    Name = propertyInfo.Name,
                    Type = propertyInfo.PropertyType,
                    EntityType = entityType,
                    RelationalKey = useAttr?.ForeignKey,
                    Info = InitRelatedPropertyInfo(propertyInfo),
                    Options = options ?? RelationOptions.Default
                };
            }
            else if (propertyInfo.PropertyType.IsGenericType &&
                typeof(IEntitySet).IsAssignableFrom(propertyInfo.PropertyType))
            {
                //实体集属性
                property = new EntitySetProperty
                {
                    RelationalType = propertyInfo.PropertyType.GetGenericArguments()[0],
                    Name = propertyInfo.Name,
                    Type = propertyInfo.PropertyType,
                    EntityType = entityType,
                    RelationalKey = useAttr?.ForeignKey,
                    Info = InitRelatedPropertyInfo(propertyInfo),
                    Options = options ?? RelationOptions.Default
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            return RegisterProperty(entityType, property);
        }

        /// <summary>
        /// 初始化属性映射信息。
        /// </summary>
        /// <param name="info">实体映射信息。</param>
        /// <param name="propertyInfo">属性信息。</param>
        /// <returns></returns>
        private static PropertyMapInfo InitPropertyInfo(PropertyMapInfo info, PropertyInfo propertyInfo)
        {
            if (info == null)
            {
                info = new PropertyMapInfo();
            }

            if (string.IsNullOrEmpty(info.FieldName))
            {
                info.FieldName = propertyInfo.Name;
            }

            info.ReflectionInfo = propertyInfo;
            return info;
        }

        private static IProperty InitProperty(Type entityType, IProperty property)
        {
            if (property.EntityType == null)
            {
                property.EntityType = entityType;
            }

            if (property.Info == null)
            {
                property.Info = new PropertyMapInfo();
            }

            if (string.IsNullOrEmpty(property.Info.FieldName))
            {
                property.Info.FieldName = property.Name;
            }

            if (property.Info.ReflectionInfo == null)
            {
                var propertyInfo = entityType.GetProperty(property.Name);
                property.Info.ReflectionInfo = propertyInfo ?? throw new PropertyNotFoundException(property.Name);
                property.Type = propertyInfo.PropertyType;
            }

            if (property.Info.DataType == null)
            {
                property.Info.DataType = property.Type.GetDbType();
            }

            if (IsNeedCorrectDefaultValue(property))
            {
                property.Info.DefaultValue.Correct(property.Type);
            }

            return property;
        }

        /// <summary>
        /// 初始化属性映射信息。
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private static PropertyMapInfo InitRelatedPropertyInfo(PropertyInfo propertyInfo)
        {
            return new PropertyMapInfo { FieldName = NullFieldName, ReflectionInfo = propertyInfo };
        }

        /// <summary>
        /// 判断属性是否需要纠正默认值的类型。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static bool IsNeedCorrectDefaultValue(IProperty property)
        {
            return !PropertyValue.IsEmpty(property.Info.DefaultValue) &&
                (property.Type.IsEnum || property.Type == typeof(bool) || property.Type == typeof(bool?));
        }

        private static void RegisterProperty(Type entityType, PropertyInfo property)
        {
            //关联属性，即关联实体或子实体集属性
            if (typeof(IEntity).IsAssignableFrom(property.PropertyType) ||
                typeof(IEntitySet).IsAssignableFrom(property.PropertyType))
            {
                var mapping = property.GetCustomAttributes<PropertyMappingAttribute>().FirstOrDefault();
                var options = mapping != null && mapping.GetFlag(PropertyMappingAttribute.SetMark.LoadBehavior) ?
                    new RelationOptions(mapping.LoadBehavior) : null;

                RegisterSupposedProperty(property.Name, property.PropertyType, entityType, options: options);
            }
            else
            {
                var gp = new GeneralProperty()
                {
                    Name = property.Name,
                    Type = property.PropertyType,
                    EntityType = entityType,
                    Info = new PropertyMapInfo { ReflectionInfo = property, FieldName = property.Name }
                };

                var mapping = property.GetCustomAttributes<PropertyMappingAttribute>().FirstOrDefault();
                if (mapping != null)
                {
                    InitMapInfo(mapping, gp.Info);
                }

                RegisterProperty(entityType, gp);
            }
        }

        /// <summary>
        /// 根据映射特性设置属性的映射信息。
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="mapInfo"></param>
        private static void InitMapInfo(PropertyMappingAttribute mapping, PropertyMapInfo mapInfo)
        {
            mapInfo.FieldName = mapping.ColumnName;
            mapInfo.Description = mapping.Description;
            mapInfo.GenerateType = mapping.GenerateType;

            if (mapping.GetFlag(PropertyMappingAttribute.SetMark.DataType))
            {
                mapInfo.DataType = mapping.DataType;
            }

            if (mapping.GetFlag(PropertyMappingAttribute.SetMark.IsPrimaryKey))
            {
                mapInfo.IsPrimaryKey = mapping.IsPrimaryKey;
            }

            if (mapping.GetFlag(PropertyMappingAttribute.SetMark.IsDeletedKey))
            {
                mapInfo.IsDeletedKey = mapping.IsDeletedKey;
            }

            if (mapping.DefaultValue != null)
            {
                mapInfo.DefaultValue = PropertyValue.NewValue(mapping.DefaultValue, mapInfo.ReflectionInfo.PropertyType);
            }

            if (mapping.GetFlag(PropertyMappingAttribute.SetMark.Length))
            {
                mapInfo.Length = mapping.Length;
            }

            if (mapping.GetFlag(PropertyMappingAttribute.SetMark.Precision))
            {
                mapInfo.Precision = mapping.Precision;
            }

            if (mapping.GetFlag(PropertyMappingAttribute.SetMark.Scale))
            {
                mapInfo.Scale = mapping.Scale;
            }
        }

        /// <summary>
        /// 在表达式中搜索属性的信息。
        /// </summary>
        private class PropertySearchVisitor : Fireasy.Common.Linq.Expressions.ExpressionVisitor
        {
            private PropertyInfo propertyInfo;
            private Type entityType;

            internal static PropertyInfo FindProperty(Expression expression)
            {
                var lambda = expression as LambdaExpression;
                if (lambda == null)
                {
                    return null;
                }

                var visitor = new PropertySearchVisitor { entityType = lambda.Parameters[0].Type };
                visitor.Visit(expression);
                return visitor.propertyInfo;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.DeclaringType == entityType &&
                    node.Member is PropertyInfo)
                {
                    propertyInfo = node.Member.As<PropertyInfo>();
                    return node;
                }

                return base.VisitMember(node);
            }
        }
    }
}
