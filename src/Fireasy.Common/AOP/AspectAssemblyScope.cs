// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;

namespace Fireasy.Common.Aop
{
    public sealed class AspectAssemblyScope : Scope<AspectAssemblyScope>
    {
        public AspectAssemblyScope(string assemblyName)
        {
            AssemblyBuilder = new DynamicAssemblyBuilder(assemblyName);
        }

        internal DynamicAssemblyBuilder AssemblyBuilder { get; }
    }
}
