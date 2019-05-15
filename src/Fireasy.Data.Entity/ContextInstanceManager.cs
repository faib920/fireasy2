// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Security;
using System.Linq;

namespace Fireasy.Data.Entity
{
    internal static class ContextInstanceManager
    {
        private static SafetyDictionary<string, EntityContextInitializeContext> instances = new SafetyDictionary<string, EntityContextInitializeContext>();

        internal static EntityContextInitializeContext Get(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                return null;
            }

            if (instances.TryGetValue(instanceName, out EntityContextInitializeContext value))
            {
                return value;
            }

            return null;
        }

        internal static string TryAdd(EntityContextInitializeContext context)
        {
            lock (instances)
            {
                var item = instances.FirstOrDefault(s => s.Value == context);
                if (!string.IsNullOrEmpty(item.Key))
                {
                    return item.Key;
                }

                var key = RandomGenerator.Create();
                instances.TryAdd(key, context);
                return key;
            }
        }
    }
}
