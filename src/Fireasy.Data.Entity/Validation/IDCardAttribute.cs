// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 对人华人民共和国居民身份证进行验证。
    /// </summary>
    public class IDCardAttribute : ValidationAttribute
    {
        private readonly bool _simple;

        /// <summary>
        /// 初始化 <see cref="IDCardAttribute"/> 类的新实例。
        /// </summary>
        public IDCardAttribute()
        {
            ErrorMessage = SR.GetString(SRKind.IDCardValideError);
        }

        /// <summary>
        /// 初始化 <see cref="IDCardAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="simple">采用简便的正则表达式。</param>
        public IDCardAttribute(bool simple)
            : this()
        {
            _simple = simple;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage, name);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || CheckIDCard18(value.ToString(), _simple))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        /// <summary>
        /// 验证18位身份证号。
        /// </summary>
        /// <param name="idcard"></param>
        /// <returns></returns>
        private static bool CheckIDCard18(string idcard, bool simple)
        {
            //正则验证
            if (!Regex.IsMatch(@"^[1-9]\d{5}(18|19|([23]\d))\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$", idcard))
            {
                return false;
            }

            if (simple)
            {
                return true;
            }

            var address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";

            //省份验证
            if (address.IndexOf(idcard.Remove(2)) == -1)
            {
                return false;
            }

            //生日验证
            if (DateTime.TryParseExact(idcard.Substring(6, 8), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _) == false)
            {
                return false;
            }

            var arrVarifyCode = "1,0,X,9,8,7,6,5,4,3,2".Split(',');
            var Wi = "7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2".Split(',');
            var Ai = idcard.Remove(17).ToCharArray();
            var sum = 0;

            for (var i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }

            Math.DivRem(sum, 11, out int y);

            if (arrVarifyCode[y] != idcard.Substring(17, 1).ToUpper())
            {
                return false;//校验码验证
            }

            return true;//符合GB11643-1999标准
        }
    }
}
