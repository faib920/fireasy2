// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 使用 <see cref="ValidationUnity"/> 类对属性的赋值进行验证时，如果值未通过其中一个验证器，则引发此异常。无法继承此类。
    /// </summary>
    public sealed class EntityInvalidateException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="EntityInvalidateException"/> 类的新实例。
        /// </summary>
        /// <param name="propertyErrors"></param>
        /// <param name="errors"></param>
        public EntityInvalidateException(Dictionary<IProperty, IList<ValidationErrorResult>> propertyErrors, IList<string> errors)
            : base(GetMessage(propertyErrors, errors))
        {
            PropertyErrors = propertyErrors;
            Errors = errors;
        }

        /// <summary>
        /// 初始化 <see cref="EntityInvalidateException"/> 类的新实例。
        /// </summary>
        /// <param name="exception"></param>
        public EntityInvalidateException(Exception exception)
            : base(string.Empty, exception)
        {
        }

        /// <summary>
        /// 获取实体全局验证的错误信息。
        /// </summary>
        public IList<string> Errors { get; }

        /// <summary>
        /// 获取每一个属性验证的错误信息。
        /// </summary>
        public Dictionary<IProperty, IList<ValidationErrorResult>> PropertyErrors { get; set; }

        private static string GetMessage(Dictionary<IProperty, IList<ValidationErrorResult>> propertyErrors, IList<string> errors)
        {
            var sb = new StringBuilder();
            sb.AppendLine(SR.GetString(SRKind.EntityInvalidate));
            foreach (var error in errors)
            {
                sb.AppendLine(error);
            }

            foreach (var error in propertyErrors)
            {
                foreach (var v in error.Value)
                {
                    sb.AppendLine($" {v.ErrorMessage}");
                }
            }

            return sb.ToString();
        }
    }
}
