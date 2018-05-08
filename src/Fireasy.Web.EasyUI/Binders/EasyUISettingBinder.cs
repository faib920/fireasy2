// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Fireasy.Web.EasyUI.Binders
{
    /// <summary>
    /// <see cref="NumberBoxSettings"/> 的绑定者。
    /// </summary>
    public class NumberBoxSettingBinder : ISettingsBinder
    {
        public bool CanBind(ISettingsBindable settings)
        {
            return typeof(NumberBoxSettings).IsAssignableFrom(settings.GetType());
        }

        public void Bind(Type modelType, string propertyName, ISettingsBindable settings)
        {
            var nsettings = settings as NumberBoxSettings;
            if (!typeof(IEntity).IsAssignableFrom(modelType))
            {
                return;
            }

            //获取对应的依赖属性
            var property = PropertyUnity.GetProperty(modelType, propertyName);
            if (property == null)
            {
                return;
            }

            nsettings.Precision = property.Info.Scale;

            //找 RangeAttribute 特性
            var range = ValidationUnity.GetValidations(property).Where(s => s is RangeAttribute).Cast<RangeAttribute>().FirstOrDefault();
            if (range != null)
            {
                nsettings.Min = range.Minimum.To<decimal?>();
                nsettings.Max = range.Maximum.To<decimal?>();
            }
        }
    }

    /// <summary>
    /// <see cref="ValidateBoxSettings"/> 的绑定者。
    /// </summary>
    public class ValidateBoxSettingBinder : ISettingsBinder
    {
        public bool CanBind(ISettingsBindable settings)
        {
            return typeof(ValidateBoxSettings).IsAssignableFrom(settings.GetType());
        }

        public void Bind(Type modelType, string propertyName, ISettingsBindable settings)
        {
            //模型类型必须实现自 IEntity
            if (!typeof(IEntity).IsAssignableFrom(modelType))
            {
                return;
            }

            //获取对应的依赖属性
            var property = PropertyUnity.GetProperty(modelType, propertyName);
            if (property == null)
            {
                return;
            }

            var vsettings = settings as ValidateBoxSettings;
            //获取依赖属性所指定的验证特性
            var validTypes = vsettings.ValidType == null ? new List<string>() :
                new List<string>(vsettings.ValidType);

            foreach (var validation in ValidationUnity.GetValidations(property))
            {
                ParseValidation(vsettings, validation, validTypes);
            }

            vsettings.ValidType = validTypes.ToArray();
        }

        /// <summary>
        /// 解析 <see cref="ValidationAttribute"/> 对象。
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="validation">要解析的 <see cref="ValidationAttribute"/> 对象。</param>
        /// <param name="validTypes">如果 <paramref name="validation"/>  能与 EasyUI 的客户端验证所对应，则添加到 validType 属性中。</param>
        protected virtual void ParseValidation(ValidateBoxSettings settings, ValidationAttribute validation, List<string> validTypes)
        {
            //必填验证特性
            if (validation is RequiredAttribute required)
            {
                settings.Required = true;
                return;
            }

            //长度验证特性
            if (validation is StringLengthAttribute stringLength)
            {
                validTypes.Add(string.Format("length[{0},{1}]", stringLength.MinimumLength, stringLength.MaximumLength));
                return;
            }

            //电话验证特性
            if (validation is TelphoneAttribute telphone)
            {
                validTypes.Add("phone");
                return;
            }

            //手机验证特性
            if (validation is MobileAttribute mobile)
            {
                validTypes.Add("mobile");
                return;
            }

            //手机或电话验证特性
            if (validation is TelphoneOrMobileAttribute telOrMobile)
            {
                validTypes.Add("phoneOrMobile");
                return;
            }

            //邮箱验证特性
            if (validation is EmailAttribute email)
            {
                validTypes.Add("email");
                return;
            }
        }
    }
}