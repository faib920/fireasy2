// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fireasy.Data.Entity
{
    internal class EntityPropertyFieldMappingResolver
    {
        private static SafetyDictionary<Type, List<PropertyFieldMapping>> cache = new SafetyDictionary<Type, List<PropertyFieldMapping>>();

        internal static IEnumerable<PropertyFieldMapping> GetDbMapping(Type entityType)
        {
            return cache.GetOrAdd(entityType, key =>
            {
                return (from s in PropertyUnity.GetLoadedProperties(key)
                       where !s.Info.IsPrimaryKey || (s.Info.IsPrimaryKey && s.Info.GenerateType != IdentityGenerateType.AutoIncrement)
                       let name = string.IsNullOrEmpty(s.Info.FieldName) ? s.Name : s.Info.FieldName
                       let dbType = s.Info.DataType == null ? DbType.String : s.Info.DataType.Value
                       select new PropertyFieldMapping(s.Name, s.Info.FieldName, s.Type, dbType)
                       {
                           ValueFunc = e => PropertyValue.GetValueSafely(((IEntity)e).GetValue(s))
                       }).ToList();
            });
        }
    }
}
