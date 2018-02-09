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
using Fireasy.Common.Extensions;

namespace Fireasy.Common
{
    /// <summary>
    /// 一个抽象类，在当前线程内标识一组用户定义的数据，这些数据在此线程块内唯一共享。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Scope<T> : IDisposable where T : Scope<T>
    {
        private readonly Dictionary<string, object> dataCache = new Dictionary<string, object>();
        private readonly bool isSingleton;
        private bool isDisposed;

        [ThreadStatic]
        private static Stack<T> Bstack = new Stack<T>();

        /// <summary>
        /// 获取当前线程范围内的当前实例。
        /// </summary>
        public static T Current
        {
            get
            {
                var stack = GetScopeStack();
                return stack.Count == 0 ? null : stack.Peek();
            }
        }

        /// <summary>
        /// 初始化类的新实例。
        /// </summary>
        /// <param name="singleton">是否为单例模式。</param>
        protected Scope(bool singleton = true)
        {
            isSingleton = singleton;
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
            dataCache.AddOrReplace(key, data);
        }

        /// <summary>
        /// 获取当前范围内指定键名的数据。
        /// </summary>
        /// <typeparam name="TData">附加数据的类型。</typeparam>
        /// <param name="key">键名。</param>
        /// <returns>返回附加的数据（如果存在）。</returns>
        public TData GetData<TData>(string key)
        {
            dataCache.TryGetValue(key, out object data);

            if (data is TData)
            {
                return (TData)data;
            }

            return default(TData);
        }

        /// <summary>
        /// 清除当前范围内的所有数据。
        /// </summary>
        public void ClearData()
        {
            dataCache.Clear();
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

            foreach (var key in keys.Where(key => dataCache.ContainsKey(key)))
            {
                dataCache.Remove(key);
            }
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                var stack = GetScopeStack();
                if (stack.Count > 0)
                {
                    if (isSingleton)
                    {
                        //单例模式下，要判断是否与 current 相等
                        if (stack.Peek().Equals(this))
                        {
                            stack.Pop();
                        }
                    }
                    else
                    {
                        stack.Pop();
                    }
                }
            }

            isDisposed = true;
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private static Stack<T> GetScopeStack()
        {
            return Bstack ?? (Bstack = new Stack<T>());
        }
    }
}
