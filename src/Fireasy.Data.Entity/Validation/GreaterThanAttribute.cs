// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 提供一个两值比较（大于另外一个）的验证方法
    /// </summary>
    public class GreaterThanAttribute : ValidationAttribute
    {
        /// <summary>
        /// 初始化 <see cref="GreaterThanAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="otherProperty">另外一个属性。</param>
        public GreaterThanAttribute(string otherProperty)
        {
            OtherProperty = otherProperty;
        }

        /// <summary>
        /// 获取或设置允许另外一个值为最小值。
        /// </summary>
        public bool AllowEqualsMinValue { get; set; }

        /// <summary>
        /// 获取或设置要比较的另外一个属性名称。
        /// </summary>
        public string OtherProperty { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IComparable comparable && comparable != null && !string.IsNullOrEmpty(OtherProperty))
            {
                //如果等于最小值
                if (EqualsMinValue(comparable))
                {
                    return ValidationResult.Success;
                }

                //获取另外一个属性值
                var property = validationContext.ObjectType.GetProperty(OtherProperty);
                if (property != null && property.PropertyType == value.GetType())
                {
                    var otherValue = property.GetValue(validationContext.ObjectInstance);
                    if (otherValue != null && (AllowEqualsMinValue || !EqualsMinValue((IComparable)otherValue)))
                    {
                        return comparable.CompareTo(otherValue) > 0 ? ValidationResult.Success : new ValidationResult(ErrorMessage);
                    }
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// 判断是否等于最小值。
        /// </summary>
        /// <param name="comparable"></param>
        /// <returns></returns>
        private bool EqualsMinValue(IComparable comparable)
        {
            var field = comparable.GetType().GetField("MinValue");
            return field != null && comparable.CompareTo(field.GetValue(null)) == 0;
        }
    }
}
