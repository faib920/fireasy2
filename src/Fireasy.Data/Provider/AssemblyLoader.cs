// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data.Common;
using System.Reflection;
using Fireasy.Common.Extensions;

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
        /// <returns></returns>
        public static DbProviderFactory Load(string typeName)
        {
            var type = Type.GetType(typeName, false, true);
            if (type != null)
            {
                return type.New<DbProviderFactory>();
            }
            return null;
        }

        /// <summary>
        /// 根据类命名和静态成员名加载 <see cref="DbProviderFactory"/> 对象。
        /// </summary>
        /// <param name="typeName">类的全名。</param>
        /// <param name="fieldName">静态的实例成员名称，如 Instance。</param>
        /// <returns></returns>
        public static DbProviderFactory Load(string typeName, string fieldName)
        {
            var type = Type.GetType(typeName, false, true);
            if (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                if (field != null)
                {
                    return field.GetValue(null) as DbProviderFactory;
                }
            }
            return null;
        }
    }
}
