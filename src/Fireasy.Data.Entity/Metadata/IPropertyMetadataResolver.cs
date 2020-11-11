// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata
{
    /// <summary>
    /// 可元数据化的属性解析器。
    /// </summary>
    public interface IPropertyMetadataResolver
    {
        /// <summary>
        /// 获取类型的属性。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetProperties(Type entityType);
    }

    internal class DefaultPropertyMetadataResolver : IPropertyMetadataResolver
    {
        public IEnumerable<PropertyInfo> GetProperties(Type entityType)
        {
            return entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(s => s.GetMethod != null && s.GetMethod.IsVirtual && !s.GetMethod.IsFinal);
        }
    }
}
