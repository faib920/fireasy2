// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Fireasy.Common.Dynamic
{
    public sealed class DynamicManager
    {
        private Dictionary<string, CallSite<Func<CallSite, object, object>>> getCallSites = new Dictionary<string, CallSite<Func<CallSite, object, object>>>();
        private Dictionary<string, CallSite<Func<CallSite, object, object, object>>> setCallSites = new Dictionary<string, CallSite<Func<CallSite, object, object, object>>>();
        private object ErrorResult = new object();

        /// <summary>
        /// 获取动态对象中指定名称的属性值。
        /// </summary>
        /// <param name="dynamicProvider">一个动态对象。</param>
        /// <param name="name">属性的名称。</param>
        /// <returns></returns>
        public object GetMember(IDynamicMetaObjectProvider dynamicProvider, string name)
        {
            if (TryGetMember(dynamicProvider, name, out object value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// 设置动态对象中指定名称的属性值。
        /// </summary>
        /// <param name="dynamicProvider">一个动态对象。</param>
        /// <param name="name">属性的名称。</param>
        /// <returns></returns>
        public void SetMember(IDynamicMetaObjectProvider dynamicProvider, string name, object value)
        {
            TrySetMember(dynamicProvider, name, value);
        }

        /// <summary>
        /// 尝试获取动态对象中指定名称的属性值。
        /// </summary>
        /// <param name="dynamicProvider">一个动态对象。</param>
        /// <param name="name">属性的名称。</param>
        /// <param name="value">返回值。</param>
        /// <returns></returns>
        public bool TryGetMember(IDynamicMetaObjectProvider dynamicProvider, string name, out object value)
        {
            if (!getCallSites.TryGetValue(name, out CallSite<Func<CallSite, object, object>> callSite))
            {
                callSite = CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember((GetMemberBinder)BinderWrapper.GetMember(name)));
            }

            var result = callSite.Target(callSite, dynamicProvider);

            if (!ReferenceEquals(result, ErrorResult))
            {
                value = result;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// 尝试设置动态对象中指定名称的属性值。
        /// </summary>
        /// <param name="dynamicProvider">一个动态对象。</param>
        /// <param name="name">属性的名称。</param>
        /// <param name="value">设置值。</param>
        /// <returns></returns>
        public bool TrySetMember(IDynamicMetaObjectProvider dynamicProvider, string name, object value)
        {
            if (!setCallSites.TryGetValue(name, out CallSite<Func<CallSite, object, object, object>> callSite))
            {
                callSite = CallSite<Func<CallSite, object, object, object>>.Create(new NoThrowSetBinderMember((SetMemberBinder)BinderWrapper.SetMember(name)));
            }

            var result = callSite.Target(callSite, dynamicProvider, value);

            return !ReferenceEquals(result, ErrorResult);
        }
    }
}
