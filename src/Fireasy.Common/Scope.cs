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
#if !NET45
using System.Threading;
#endif
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;

namespace Fireasy.Common
{
    /// <summary>
    /// 一个抽象类，在当前线程内标识一组用户定义的数据，这些数据在此线程块内唯一共享。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Scope<T> : DisposableBase where T : Scope<T>
    {
        private readonly Dictionary<string, object> _dataCache = new Dictionary<string, object>();
        private readonly bool _isSingleton;

#if !NET45
        private static readonly AsyncLocal<Stack<T>> _staticStack = new AsyncLocal<Stack<T>>();
#else
        [ThreadStatic]
        private static Stack<T> _staticStack = new Stack<T>();
#endif

        /// <summary>
        /// 获取当前线程范围内的当前实例。
        /// </summary>
        public static T Current
        {
            get
            {
                var stack = GetScopeStack();
                try
                {
                    return stack.Count == 0 ? null : stack.Peek();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 初始化 <see cref="Scope{T}"/> 类的新实例。
        /// </summary>
        /// <param name="singleton">是否为单例模式。</param>
        protected Scope(bool singleton = true)
        {
            _isSingleton = singleton;
            var stack = GetScopeStack();

            if (singleton)
            {
                if (stack.Count == 0)
                {
                    stack.Push((T)this);
                }
            }
            else
            {
                stack.Push((T)this);
            }
        }

        /// <summary>
        /// 在当前范围内添加一个数据。
        /// </summary>
        /// <typeparam name="TData">附加数据的类型。</typeparam>
        /// <param name="key">键名。</param>
        /// <param name="data">数据值。</param>
        public void SetData<TData>(string key, TData data)
        {
            _dataCache.AddOrReplace(key, data);
        }

        /// <summary>
        /// 获取当前范围内指定键名的数据。
        /// </summary>
        /// <typeparam name="TData">附加数据的类型。</typeparam>
        /// <param name="key">键名。</param>
        /// <returns>返回附加的数据（如果存在）。</returns>
        public TData GetData<TData>(string key)
        {
            _dataCache.TryGetValue(key, out object data);

            if (data is TData tdata)
            {
                return tdata;
            }

            return default;
        }

        /// <summary>
        /// 清除当前范围内的所有数据。
        /// </summary>
        public void ClearData()
        {
            _dataCache.Clear();
        }

        /// <summary>
        /// 清除当前范围内指定键名的数据。
        /// </summary>
        /// <param name="keys">一组表示键名的字符串。</param>
        public void RemoveData(params string[] keys)
        {
            if (keys == null)
            {
                return;
            }

            foreach (var key in keys.Where(key => _dataCache.ContainsKey(key)))
            {
                _dataCache.Remove(key);
            }
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            var stack = GetScopeStack();

            if (stack.Count > 0)
            {
                //单例模式下，要判断是否与 current 相等
                if (_isSingleton)
                {
                    try
                    {
                        var current = stack.Peek();
                        if (current == null || current.Equals(this))
                        {
                            stack.Pop();
                        }
                    }
                    catch { }
                }
                else
                {
                    stack.Pop();
                }
            }

            return true;
        }

        /// <summary>
        /// 在当前堆栈中查找最匹配的一个实例。
        /// </summary>
        /// <param name="predicate">用于判断是否匹配的函数。</param>
        /// <returns></returns>
        protected T Match(Func<T, bool> predicate)
        {
            var stack = GetScopeStack();
            return stack.FirstOrDefault(predicate);
        }

        private static Stack<T> GetScopeStack()
        {
#if !NET45
            if (_staticStack.Value == null)
            {
                _staticStack.Value = new Stack<T>();
            }

            return _staticStack.Value;
#else
            return _staticStack ?? (_staticStack = new Stack<T>());
#endif
        }
    }
}
