// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    internal class ScopedDictionary<TKey, TValue>
    {
        private readonly ScopedDictionary<TKey, TValue> m_previous;
        private readonly Dictionary<TKey, TValue> m_map;

        public ScopedDictionary(ScopedDictionary<TKey, TValue> previous)
        {
            m_previous = previous;
            m_map = new Dictionary<TKey, TValue>();
        }

        public ScopedDictionary(ScopedDictionary<TKey, TValue> m_previous, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
            : this(m_previous)
        {
            foreach (var p in pairs)
            {
                m_map.Add(p.Key, p.Value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            m_map.Add(key, value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            for (var scope = this; scope != null; scope = scope.m_previous)
            {
                if (scope.m_map.TryGetValue(key, out value))
                    return true;
            }
            value = default(TValue);
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            for (var scope = this; scope != null; scope = scope.m_previous)
            {
                if (scope.m_map.ContainsKey(key))
                    return true;
            }
            return false;
        }
    }
}