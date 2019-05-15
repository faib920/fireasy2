using Fireasy.Common.Emit;
using System;

namespace Fireasy.Data.Entity
{
    public interface IEntityInjection
    {
        void Inject(Type entityType, DynamicAssemblyBuilder assemblyBuilder, DynamicTypeBuilder typeBuilder);
    }
}
