// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 为属性修改完成事件提供的参数。无法继承此类。
    /// </summary>
    public sealed class PropertyChangedEventArgs : System.ComponentModel.PropertyChangedEventArgs
    {
        internal PropertyChangedEventArgs(IProperty property, PropertyValue oldValue, PropertyValue newValue)
            : base(property.Name)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// 获取所修改的属性。
        /// </summary>
        public IProperty Property { get; }

        /// <summary>
        /// 获取属性修改前的值。
        /// </summary>
        public PropertyValue OldValue { get; }

        /// <summary>
        /// 获取属性修改后的新值。
        /// </summary>
        public PropertyValue NewValue { get; }
    }
}
