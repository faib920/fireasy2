// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 用于从外部程序集中加载 <see cref="DbProviderFactory"/> 对象。无法继承此类。
    /// </summary>
    public static class AssemblyLoader
    {
        /// <summary>
        /// 根据类全名加载 <see cref="DbProviderFactory"/> 对象。
        /// </summary>
        /// <param name="typeName">类的全名。</param>
        /// <param name="valueCreator"></param>
        /// <returns></returns>
        public static bool TryLoad(string typeName, out DbProviderFactory valueCreator)
        {
            valueCreator = null;
            try
            {
                var type = Type.GetType(typeName, false, true);
                if (type != null && typeof(DbProviderFactory).IsAssignableFrom(type))
                {
                    var field = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(s => typeof(DbProviderFactory).IsAssignableFrom(s.FieldType));

                    if (field != null)
                    {
                        valueCreator = field.GetValue(null) as DbProviderFactory;
                    }
                    else
                    {
                        valueCreator = type.New<DbProviderFactory>();
                    }
                }

            }
            catch
            {
            }

            return valueCreator != null;
        }
    }
}
