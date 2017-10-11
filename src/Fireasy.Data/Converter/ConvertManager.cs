// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Drawing;
using System.Linq;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Data.Converter.Configuration;
using System.Data;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 转换器管理器。
    /// </summary>
    public static class ConvertManager
    {
        /// <summary>
        /// 根据对象类型创建相应的转换器。
        /// </summary>
        /// <param name="conversionType">要转换的数据类型。</param>
        /// <returns>返回一个 <see cref="IValueConverter"/> 实例，如果未找到对应的转换器，则返回 null。</returns>
        public static IValueConverter GetConverter(Type conversionType)
        {
            //从配置里找 IValueConverter 对象
            var section = ConfigurationUnity.GetSection<ConverterConfigurationSection>();
            if (section != null)
            {
                var setting = section.Settings.FirstOrDefault(s => s.Value.SourceType == conversionType).Value;
                if (setting != null)
                {
                    return setting.ConverterType.New<IValueConverter>();
                }
            }

            if (conversionType.IsArray)
            {
                return new ArrayConverter(conversionType.GetElementType());
            }

            if (conversionType == typeof(Color))
            {
                return new ColorConverter();
            }

            if (conversionType == typeof(Point))
            {
                return new PointConverter();
            }

            if (conversionType == typeof(Rectangle))
            {
                return new RectangleConverter();
            }

            if (conversionType == typeof(Size))
            {
                return new SizeConverter();
            }

            if (conversionType == typeof(Image)
                || conversionType == typeof(Bitmap))
            {
                return new ImageConverter();
            }

            if (conversionType == typeof(Font))
            {
                return new FontConverter();
            }

            if (typeof(Exception).IsAssignableFrom(conversionType))
            {
                return new ExceptionConverter();
            }

            return null;
        }

        /// <summary>
        /// 判断指定的类型是否支持转换。
        /// </summary>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static bool CanConvert(Type conversionType)
        {
            //从配置里找 IValueConverter 对象
            var section = ConfigurationUnity.GetSection<ConverterConfigurationSection>();
            if (section != null)
            {
                var setting = section.Settings.FirstOrDefault(s => s.Value.SourceType == conversionType).Value;
                if (setting != null)
                {
                    return true;
                }
            }

            if (conversionType.IsArray 
                ||conversionType == typeof(Color)
                || conversionType == typeof(Point)
                || conversionType == typeof(Rectangle)
                || conversionType == typeof(Size)
                || conversionType == typeof(Image)
                || conversionType == typeof(Bitmap)
                || conversionType == typeof(Font)
                || typeof(Exception).IsAssignableFrom(conversionType))
            {
                return true;
            }

            return false;
        }
    }
}
