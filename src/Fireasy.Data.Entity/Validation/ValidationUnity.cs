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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 提供对实体及属性值验证的管理单元。
    /// </summary>
    public static class ValidationUnity
    {
        private static readonly SafetyDictionary<Type, Dictionary<string, List<ValidationAttribute>>> propertyValidations =
            new SafetyDictionary<Type, Dictionary<string, List<ValidationAttribute>>>();
        private static readonly SafetyDictionary<Type, List<ValidationAttribute>> entityValidations =
            new SafetyDictionary<Type, List<ValidationAttribute>>();

        /// <summary>
        /// 对实体指定属性的值进行验证。
        /// </summary>
        /// <param name="entity">要验证的实体。</param>
        /// <param name="property">要验证的实体属性。</param>
        /// <param name="value">将赋给属性的值。</param>
        /// <param name="callback">默认为 null。当实体属性验证失败时，可以使用该回调方法处理异常信息。</param>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> 或 <paramref name="property"/> 参数为 null。</exception>
        /// <exception cref="PropertyInvalidateException">实体属性验证失败且 <paramref name="callback"/> 参数为 null。</exception>
        /// <returns>验证通过为 true，否则为 false。</returns>
        public static bool Validate(IEntity entity, IProperty property, PropertyValue value, Action<IProperty, IList<string>> callback = null)
        {
            Guard.ArgumentNull(entity, nameof(entity));
            Guard.ArgumentNull(property, nameof(property));

            var validations = GetValidations(property);
            if (validations == null)
            {
                return true;
            }

            var isValid = true;
            var messages = new List<string>();

            //获取属性真实的值
            var relValue = PropertyValue.GetValue(value);
            if (relValue != null && relValue is IFormattable formatter)
            {
                relValue = relValue.ToString();
            }

            var context = CreateContext(entity, property);

            foreach (var validation in validations)
            {
                try
                {
                    var result = validation.GetValidationResult(relValue, context);

                    //验证失败，记录提示信息
                    if (result != ValidationResult.Success)
                    {
                        messages.Add(result.ErrorMessage);
                        isValid = false;
                    }
                }
                catch (Exception exp)
                {
                    throw new PropertyInvalidateException(property, exp);
                }
            }

            if (!isValid)
            {
                if (callback == null)
                {
                    throw new PropertyInvalidateException(property, messages);
                }

                //回调，将当前属性的异常信息通知调用程序
                callback(property, messages);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 对实体指定属性的值进行验证。
        /// </summary>
        /// <param name="property">要验证的实体属性。</param>
        /// <param name="value">将赋给属性的值。</param>
        /// <param name="callback">默认为 null。当实体属性验证失败时，可以使用该回调方法处理异常信息。</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> 参数为 null。</exception>
        /// <exception cref="PropertyInvalidateException">实体属性验证失败且 <paramref name="callback"/> 参数为 null。</exception>
        /// <returns>验证通过为 true，否则为 false。</returns>
        public static bool Validate(IProperty property, object value, Action<IProperty, IList<string>> callback = null)
        {
            Guard.ArgumentNull(property, nameof(property));

            var validations = GetValidations(property);
            if (validations == null)
            {
                return true;
            }

            var isValid = true;
            var messages = new List<string>();

            //获取属性真实的值

            foreach (var validation in validations)
            {
                var displayName = property.Info.Description;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = property.Name;
                }

                try
                {
                    validation.Validate(value, displayName);
                }
                catch (ValidationException exp)
                {
                    messages.Add(exp.Message);
                    isValid = false;
                }
                catch (Exception exp)
                {
                    throw new PropertyInvalidateException(property, exp);
                }
            }

            if (!isValid)
            {
                if (callback == null)
                {
                    throw new PropertyInvalidateException(property, messages);
                }

                //回调，将当前属性的异常信息通知调用程序
                callback(property, messages);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 对实体进行属性和逻辑验证，如果验证失败，则抛出 <see cref="EntityInvalidateException"/> 异常。
        /// </summary>
        /// <param name="entity">要验证的实体。</param>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> 参数为 null。</exception>
        /// <exception cref="EntityInvalidateException">实体验证失败。</exception>
        public static void Validate(IEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (entity.EntityState == EntityState.Unchanged)
            {
                return;
            }

            var isValid = true;
            var propertyErrors = new Dictionary<IProperty, IList<string>>();
            var errors = new List<string>();

            var proValidations = GetPropertyValidations(entity.EntityType).AsEnumerable();

            if (entity.EntityState == EntityState.Modified)
            {
                var modifiedKeys = entity.GetModifiedProperties();
                proValidations = proValidations.Where(s => modifiedKeys.Contains(s.Key));
            }

            //获取定义了验证特性“属性/验证特性”字典
            foreach (var kvp in proValidations)
            {
                var property = PropertyUnity.GetProperty(entity.EntityType, kvp.Key);
                if (property == null)
                {
                    continue;
                }

                var value = entity.EntityType.IsNotCompiled() ? entity.GetValue(property) : entity.GetDirectValue(property);

                isValid &= Validate(entity, property, value, (p, messages) => propertyErrors.TryAdd(property, messages));
            }

            //获取于实体类的验证特性，而不是属性的
            var validations = GetValidations(entity.EntityType);
            var context = new ValidationContext(entity, null, null);

            foreach (var validation in validations)
            {
                try
                {
                    var reuslt = validation.GetValidationResult(entity, context);
                    //验证失败，记录提示信息
                    if (reuslt != ValidationResult.Success)
                    {
                        isValid = false;
                        errors.Add(reuslt.ErrorMessage);
                    }
                }
                catch (Exception exp)
                {
                    throw new EntityInvalidateException(exp);
                }
            }

            if (!isValid)
            {
                throw new EntityInvalidateException(propertyErrors, errors);
            }
        }

        /// <summary>
        /// 获取给实体属性定义的所有验证器序列，这些验证器通过 <see cref="MetadataTypeAttribute"/> 特性所指定的类型列定义。
        /// </summary>
        /// <param name="property">要获取验证器的实体属性。</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> 参数为 null。</exception>
        /// <returns>一个 <see cref="ValidationAttribute"/> 的序列。</returns>
        public static IEnumerable<ValidationAttribute> GetValidations(IProperty property)
        {
            Guard.ArgumentNull(property, nameof(property));

            var entityType = property.EntityType;
            var propertyName = property.Name;

            var pdic = propertyValidations.GetOrAdd(entityType, () => GetPropertyValidations(entityType));

            if (!pdic.TryGetValue(propertyName, out List<ValidationAttribute> attrs))
            {
                attrs = pdic.TryGetValue(propertyName, () => new List<ValidationAttribute>());
            }

            return attrs;
        }

        /// <summary>
        /// 获取给实体定义的所有验证器序列，这些验证器通过 <see cref="MetadataTypeAttribute"/> 特性所指定的类型列定义。
        /// </summary>
        /// <param name="entityType">一个实体的类型。</param>
        /// <exception cref="ArgumentNullException"><paramref name="entityType"/> 参数为 null。</exception>
        /// <returns>一个 <see cref="ValidationAttribute"/> 的序列。</returns>
        public static IEnumerable<ValidationAttribute> GetValidations(Type entityType)
        {
            Guard.ArgumentNull(entityType, nameof(entityType));

            return entityValidations.GetOrAdd(entityType, () => GetEntityValidations(entityType));
        }

        /// <summary>
        /// 获取指定实体的 <see cref="ValidationAttribute"/> 定义集。
        /// </summary>
        /// <param name="entityType">一个实体的类型。</param>
        /// <returns></returns>
        private static List<ValidationAttribute> GetEntityValidations(Type entityType)
        {
            var metadataType = entityType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
            return (metadataType != null ? metadataType.MetadataClassType : entityType).GetCustomAttributes<ValidationAttribute>().ToList();
        }

        /// <summary>
        /// 获取指定属性名称的 <see cref="ValidationAttribute"/> 定义集。
        /// </summary>
        /// <param name="entityType">一个实体的类型。</param>
        /// <returns></returns>
        private static Dictionary<string, List<ValidationAttribute>> GetPropertyValidations(Type entityType)
        {
            var dictionary = new Dictionary<string, List<ValidationAttribute>>();
            var metadataType = entityType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
            if (metadataType != null &&
                typeof(IMetadataContainer).IsAssignableFrom(metadataType.MetadataClassType))
            {
                return metadataType.MetadataClassType.New<IMetadataContainer>().InitializeRules();
            }

            var properties = metadataType == null ?
                PropertyUnity.GetProperties(entityType).Select(s => s.Info.ReflectionInfo) :
                metadataType.MetadataClassType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<ValidationAttribute>().ToList();
                if (attributes.Count == 0)
                {
                    continue;
                }

                IProperty dp;
                if ((dp = CheckPropertyValid(entityType, property)) == null)
                {
                    continue;
                }

                var list = dictionary.TryGetValue(property.Name, () => new List<ValidationAttribute>());
                MarkValidationProperty(dp, attributes);
                list.AddRange(attributes);
            }

            return dictionary;
        }

        /// <summary>
        /// 检查属性是否有效。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static IProperty CheckPropertyValid(Type entityType, PropertyInfo property)
        {
            var dp = PropertyUnity.GetProperty(entityType, property.Name);
            if (dp == null)
            {
                return null;
            }

            var pInfo = dp.Info;

            //删除标记、自动生成和有默认值的不进行验证
            if (pInfo.IsDeletedKey ||
                pInfo.GenerateType != IdentityGenerateType.None ||
                !PropertyValue.IsEmpty(pInfo.DefaultValue))
            {
                return null;
            }

            return dp;
        }

        /// <summary>
        /// 为一组 <see cref="ValidationAttribute"/> 实现接口 <see cref="IPropertyMarker"/>。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="validations"></param>
        private static void MarkValidationProperty(IProperty property, IEnumerable<ValidationAttribute> validations)
        {
            validations.ForEach(val => val.As<IPropertyMarker>(mark => mark.Property = property));
        }

        /// <summary>
        /// 创建验证上下文对象。
        /// </summary>
        /// <param name="entity">当前验证的实体。</param>
        /// <param name="property">当前验证的属性。</param>
        /// <returns></returns>
        private static ValidationContext CreateContext(IEntity entity, IProperty property)
        {
            //显示在异常信息中的字段名称
            var displayName = property.Info.Description;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = property.Name;
            }

            return new ValidationContext(entity, null, null)
            {
                DisplayName = displayName,
                MemberName = property.Name
            };
        }
    }
}
