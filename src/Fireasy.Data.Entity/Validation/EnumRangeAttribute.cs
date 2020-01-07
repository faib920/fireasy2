// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 对枚举值的取值范围进行验证。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EnumRangeAttribute : ValidationAttribute
    {
        private ReadOnlyCollection<int> values;

        /// <summary>
        /// 初始化 <see cref="EnumRangeAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="enumType">验证的枚举类型。</param>
        public EnumRangeAttribute(Type enumType)
        {
            EnumType = enumType;
        }

        /// <summary>
        /// 获取或设置验证的枚举类型。
        /// </summary>
        public Type EnumType { get; set; }

        /// <summary>
        /// 获取可取的值范围。
        /// </summary>
        public ReadOnlyCollection<int> Values
        {
            get
            {
                if (values == null)
                {
                    values = new ReadOnlyCollection<int>(EnumType.GetFields().Where(s => s.Name != "value__").Select(s => (int)s.GetValue(null)).ToList());
                }

                return values;
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (Enum.IsDefined(EnumType, value))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
