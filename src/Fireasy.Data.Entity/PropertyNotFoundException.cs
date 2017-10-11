using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 属性没有找到时引发此异常。无法继承此类。
    /// </summary>
    public sealed class PropertyNotFoundException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="PropertyNotFoundException"/> 类的新实例。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        public PropertyNotFoundException(string propertyName)
            : base(SR.GetString(SRKind.PropertyNotFound, propertyName))
        {
        }
    }
}
