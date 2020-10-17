// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using System;
using System.Reflection;
using System.Text;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// jQuery 参数抽象类。
    /// </summary>
    public abstract class SettingsBase : ITextSerializable
    {
        /// <summary>
        /// 反序列化，读取 json 文本中的属性。
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="text"></param>
        public virtual void Deserialize(ITextSerializer serializer, string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 序列化，将对象的各个属性写入 json 文本。
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public virtual string Serialize(ITextSerializer serializer)
        {
            var sb = new StringBuilder();

            //获取所有的属性
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                //读取属性值，如果
                var value = property.GetValue(this, null);
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                var name = property.Name;
                sb.AppendFormat("{0}{1}:", name.Substring(0, 1).ToLower(), name.Substring(1));
                sb.AppendFormat("{0}", property.IsDefined<EventFunctionAttribute>() ? value :
                    SerializePropertyValue(property, value, serializer));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 序列化属性的值。
        /// </summary>
        /// <param name="property">属性。</param>
        /// <param name="value">属性的值。</param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        protected virtual string SerializePropertyValue(PropertyInfo property, object value, ITextSerializer serializer)
        {
            return serializer.Serialize(value);
        }
    }
}
