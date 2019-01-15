// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// JQuery 参数的序列化。无法继承此类。
    /// </summary>
    public sealed class SettingsSerializer
    {
        /// <summary>
        /// 对 <paramref name="settings"/> 对象进行序列化。
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string Serialize(SettingsBase settings)
        {
            if (settings == null)
            {
                return string.Empty;
            }

            var option = new JsonSerializeOption { Format = JsonFormat.Object };
            var json = new JsonSerializer(option);
            json.Option.Converters.Add(new StringConverter());
            return json.Serialize(settings);
        }

        private class StringConverter : JsonConverter
        {
            public override bool CanConvert(Type type)
            {
                return type == typeof(string);
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
            {
                writer.WriteRaw($"'{obj}'");
            }
        }
    }
}
