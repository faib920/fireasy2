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
    /// 对人华人民共和国居民身份证进行验证。
    /// </summary>
    public class IDCardAttribute : ValidationAttribute
    {
        /// <summary>
        /// 初始化 <see cref="IDCardAttribute"/> 类的新实例。
        /// </summary>
        public IDCardAttribute()
        {
            ErrorMessage = SR.GetString(SRKind.IDCardValideError);
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage, name);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || CheckIDCard18(value.ToString()))
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
        private static bool CheckIDCard18(string idcard)
        {
            if (idcard.Length < 18)
            {
                return false;
            }

            if (long.TryParse(idcard.Remove(17), out long n) == false ||
                n < Math.Pow(10, 16) ||
                long.TryParse(idcard.Replace('x', '0').Replace('X', '0'), out _) == false)
            {
                return false;//数字验证
            }

            var address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";

            if (address.IndexOf(idcard.Remove(2)) == -1)
            {
                return false;//省份验证
            }

            var birth = idcard.Substring(6, 8).Insert(6, "-").Insert(4, "-");

            //生日验证
            if (DateTime.TryParse(birth, out _) == false)
            {
                return false;
            }

            var arrVarifyCode = ("1,0,X,9,8,7,6,5,4,3,2").Split(',');
            var Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
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
