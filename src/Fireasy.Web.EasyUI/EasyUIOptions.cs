#if NETCOREAPP
using Fireasy.Web.EasyUI.Binders;
using System.Collections.Generic;

namespace Fireasy.Web.EasyUI
{
    public class EasyUIOptions
    {
        internal Dictionary<string, ISettingsBinder> binders = new Dictionary<string, ISettingsBinder>();

        public void AddBinder(string name, ISettingsBinder binder)
        {
            binders.Add(name, binder);
        }
    }
}
#endif