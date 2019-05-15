using System;

namespace Fireasy.Data.Provider
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class DefaultProviderServiceAttribute : Attribute
    {
        public DefaultProviderServiceAttribute(Type defaultType)
        {
            DefaultType = defaultType;
        }

        public Type DefaultType { get; set; }
    }
}
