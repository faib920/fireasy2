// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于记录实体的属性的数据。
    /// </summary>
    [Serializable]
    internal class EntityEntry
    {
        private PropertyValue _oldValue;
        private PropertyValue _newValue;

        /// <summary>
        /// 获取或设置实体值是否已修改。
        /// </summary>
        internal bool IsModified { get; set; }

        internal static EntityEntry Modified()
        {
            return new EntityEntry { IsModified = true };
        }

        /// <summary>
        /// 重置修改状态。
        /// </summary>
        internal void Reset()
        {
            if (IsModified && _newValue != null)
            {
                _oldValue = _newValue.Clone();
                _newValue = PropertyValue.Empty;
                IsModified = false;
            }
        }

        /// <summary>
        /// 使用指定的值进行修改。
        /// </summary>
        /// <param name="value"></param>
        internal void Modify(PropertyValue value)
        {
            _newValue = value;
            IsModified = true;
        }

        /// <summary>
        /// 标识已被修改。
        /// </summary>
        internal void Modify()
        {
            if (!IsModified)
            {
                _newValue = _oldValue;
                IsModified = true;
            }
        }

        /// <summary>
        /// 获取当前值。如果正在修改，则返回新值，否则为旧值。
        /// </summary>
        /// <returns></returns>
        internal PropertyValue GetCurrentValue()
        {
            if (IsModified)
            {
                return _newValue;
            }

            return PropertyValue.IsEmpty(_newValue) ? _oldValue : _newValue;
        }

        /// <summary>
        /// 获取旧值。
        /// </summary>
        /// <returns></returns>
        internal PropertyValue GetOldValue()
        {
            return _oldValue;
        }

        /// <summary>
        /// 初始化，设置旧值。
        /// </summary>
        /// <param name="value"></param>
        internal void Initializate(PropertyValue value)
        {
            if (IsModified)
            {
                _newValue = value;
            }
            else
            {
                _oldValue = value;
            }
        }

        /// <summary>
        /// 使用旧值初始化一个 <see cref="EntityEntry"/>。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static EntityEntry InitByOldValue(PropertyValue value)
        {
            return new EntityEntry { _oldValue = value };
        }

        /// <summary>
        /// 使用新值初始化一个 <see cref="EntityEntry"/>。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static EntityEntry InitByNewValue(PropertyValue value)
        {
            return new EntityEntry { _newValue = value, IsModified = true };
        }
    }
}