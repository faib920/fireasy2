// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 用于控制循环反转对象的范围对象。
    /// </summary>
    internal class ResolveLoopScope : Scope<ResolveLoopScope>
    {
        private readonly List<Type> _types = new List<Type>();

        /// <summary>
        /// 尝试添加正在反转的类型，如果返回 false 则表示之前已经进行了反转。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool TryAddType(Type type)
        {
            if (_types.Contains(type))
            {
                return false;
            }

            _types.Add(type);
            return true;
        }
    }
}
