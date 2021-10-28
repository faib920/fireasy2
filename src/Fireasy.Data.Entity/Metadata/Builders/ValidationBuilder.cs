// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Validation;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 验证构造器。
    /// </summary>
    public class ValidationBuilder : IMetadataBuilder
    {
        private readonly PropertyMapInfo _mapInfo;

        public ValidationBuilder(PropertyMapInfo mapInfo)
        {
            _mapInfo = mapInfo;
        }

        /// <summary>
        /// 添加 <see cref="RequiredAttribute"/> 验证。
        /// </summary>
        /// <returns></returns>
        public virtual ValidationBuilder Required()
        {
            return With(new RequiredAttribute());
        }

        /// <summary>
        /// 添加 <see cref="RegularExpressionAttribute"/> 验证。
        /// </summary>
        /// <param name="pattern">正则表达式。</param>
        /// <returns></returns>
        public virtual ValidationBuilder Regular(string pattern)
        {
            return With(new RegularExpressionAttribute(pattern));
        }

        /// <summary>
        /// 添加 <see cref="StringLengthAttribute"/> 验证。
        /// </summary>
        /// <param name="maxLength">最大长度。</param>
        /// <returns></returns>
        public virtual ValidationBuilder StringLength(int maxLength)
        {
            return With(new StringLengthAttribute(maxLength));
        }

        /// <summary>
        /// 添加 <see cref="MaxLengthAttribute"/> 验证。
        /// </summary>
        /// <param name="length">长度。</param>
        /// <returns></returns>
        public virtual ValidationBuilder MaxLength(int length)
        {
            return With(new MaxLengthAttribute(length));
        }

        /// <summary>
        /// 添加 <see cref="MinLengthAttribute"/> 验证。
        /// </summary>
        /// <param name="length">长度。</param>
        /// <returns></returns>
        public virtual ValidationBuilder MinLength(int length)
        {
            return With(new MinLengthAttribute(length));
        }

        /// <summary>
        /// 添加 <see cref="RangeAttribute"/> 验证。
        /// </summary>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
        /// <returns></returns>
        public virtual ValidationBuilder Range(int min, int max)
        {
            return With(new RangeAttribute(min, max));
        }

        /// <summary>
        /// 添加 <see cref="RangeAttribute"/> 验证。
        /// </summary>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
        /// <returns></returns>
        public virtual ValidationBuilder Range(double min, double max)
        {
            return With(new RangeAttribute(min, max));
        }

        /// <summary>
        /// 添加 <see cref="EmailAttribute"/> 验证。
        /// </summary>
        /// <returns></returns>
        public virtual ValidationBuilder Email()
        {
            return With(new EmailAttribute());
        }

        /// <summary>
        /// 添加 <see cref="WebSiteAttribute"/> 验证。
        /// </summary>
        /// <returns></returns>
        public virtual ValidationBuilder WebSite()
        {
            return With(new WebSiteAttribute());
        }

        /// <summary>
        /// 添加 <see cref="IDCardAttribute"/> 验证。
        /// </summary>
        /// <param name="simple"></param>
        /// <returns></returns>
        public virtual ValidationBuilder IDCard(bool simple = false)
        {
            return With(new IDCardAttribute(simple));
        }

        /// <summary>
        /// 添加 <see cref="MobileAttribute"/> 验证。
        /// </summary>
        /// <param name="simple"></param>
        /// <returns></returns>
        public virtual ValidationBuilder Mobile(bool simple = false)
        {
            return With(new MobileAttribute(simple));
        }

        /// <summary>
        /// 添加 <see cref="TelphoneAttribute"/> 验证。
        /// </summary>
        /// <returns></returns>
        public virtual ValidationBuilder Telphone()
        {
            return With(new TelphoneAttribute());
        }

        /// <summary>
        /// 添加 <see cref="TelphoneOrMobileAttribute"/> 验证。
        /// </summary>
        /// <returns></returns>
        public virtual ValidationBuilder TelphoneOrMobile()
        {
            return With(new TelphoneOrMobileAttribute());
        }

        /// <summary>
        /// 添加 <see cref="ZipCodeAttribute"/> 验证。
        /// </summary>
        /// <returns></returns>
        public virtual ValidationBuilder ZipCode()
        {
            return With(new ZipCodeAttribute());
        }

        /// <summary>
        /// 添加指定的一个任意的验证特性。
        /// </summary>
        /// <typeparam name="TValidation"></typeparam>
        /// <returns></returns>
        public virtual ValidationBuilder With<TValidation>() where TValidation : ValidationAttribute, new()
        {
            _mapInfo?.Attributes.Add(new TValidation());
            return this;
        }

        /// <summary>
        /// 添加指定的一个任意的验证特性。
        /// </summary>
        /// <param name="validation"></param>
        /// <returns></returns>
        public virtual ValidationBuilder With(ValidationAttribute validation)
        {
            _mapInfo?.Attributes.Add(validation);
            return this;
        }

        void IMetadataBuilder.Build()
        {
        }

        internal class NullBuilder : ValidationBuilder
        {
            public NullBuilder()
                : base (null)
            {
            }
        }
    }
}
