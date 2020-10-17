// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Fireasy.Web.EasyUI.Binders;
using System.Collections.Generic;

namespace Fireasy.Web.EasyUI
{
    public class EasyUIOptions
    {
        internal Dictionary<string, ISettingsBinder> _binders = new Dictionary<string, ISettingsBinder>();

        /// <summary>
        /// 添加一个 <see cref="ISettingsBinder"/> 实例。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="binder"></param>
        public void AddBinder(string name, ISettingsBinder binder)
        {
            _binders.Add(name, binder);
        }
    }
}
#endif