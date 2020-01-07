// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 使用 <see cref="ValidationUnity"/> 类对属性的赋值进行验证时，如果值未通过其中一个验证器，则引发此异常。无法继承此类。
    /// </summary>
    public sealed class PropertyInvalidateException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="PropertyInvalidateException"/> 类的新实例。
        /// </summary>
        /// <param name="property">所验证的实体属性。</param>
        /// <param name="errors">验证器产生的错误信息列表。</param>
        public PropertyInvalidateException(IProperty property, IList<ValidationErrorResult> errors)
            : base (GetMessage(property, errors))
        {
            Property = property;
            Errors = new ReadOnlyCollection<ValidationErrorResult>(errors);
        }

        /// <summary>
        /// 初始化 <see cref="PropertyInvalidateException"/> 类的新实例。
        /// </summary>
        /// <param name="property">所验证的实体属性。</param>
        /// <param name="exp">验证器产生的错误信息列表。</param>
        public PropertyInvalidateException(IProperty property, Exception exp)
            : base (property.Name + " : " + exp.Message, exp)
        {
            Property = property;
        }

        /// <summary>
        /// 获取所验证的实体属性。
        /// </summary>
        public IProperty Property { get; private set; }

        /// <summary>
        /// 获取错误验证器产生的信息列表。
        /// </summary>
        public ReadOnlyCollection<ValidationErrorResult> Errors { get; private set; }

        private static string GetMessage(IProperty property, IList<ValidationErrorResult> errors)
        {
            var sb = new StringBuilder();
            sb.AppendLine(SR.GetString(SRKind.EntityInvalidate));
            foreach (var error in errors)
            {
                sb.AppendLine(error.ErrorMessage);
            }

            return sb.ToString();
        }
    }
}
