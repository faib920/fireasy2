// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Properties;
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体延迟加载的管理器。
    /// </summary>
    internal class EntityLzayManager
    {
        private readonly Type _entityType;
        private readonly List<string> _status = new List<string>();

        /// <summary>
        /// 初始化 <see cref="EntityLzayManager"/> 类的新实例。
        /// </summary>
        /// <param name="entityType"></param>
        public EntityLzayManager(Type entityType)
        {
            _entityType = entityType;
        }

        /// <summary>
        /// 设置指定名称的属性的值已经创建。
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        internal void SetValueCreated(string propertyName)
        {
            if (!_status.Contains(propertyName))
            {
                _status.Add(propertyName);
            }
        }

        /// <summary>
        /// 判断指定名称的属性的值是否已经创建。
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        internal bool IsValueCreated(string propertyName)
        {
            var property = PropertyUnity.GetProperty(_entityType, propertyName);
            if (property is RelationProperty)
            {
                return _status.Contains(propertyName);
            }

            return true;
        }
    }
}
