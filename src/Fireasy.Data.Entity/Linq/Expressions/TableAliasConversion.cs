// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表别名转换器。无法继承此类。
    /// </summary>
    public sealed class TableAliasConversion
    {
        private readonly Dictionary<Type, string> _aliasDict = new Dictionary<Type, string>();

        public TableAliasConversion()
        {
        }

        public TableAliasConversion(Type type, string name)
        {
            Add(type, name);
        }

        public TableAliasConversion Add(Type type, string name)
        {
            if (!_aliasDict.ContainsKey(type))
            {
                _aliasDict.Add(type, name);
            }

            return this;
        }

        public int Count => _aliasDict.Count;

        public IEnumerable<Type> Types => _aliasDict.Keys;

        public string this[Type type] => _aliasDict[type];
    }
}
