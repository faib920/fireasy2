// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Reflection.Emit;

namespace Fireasy.Common.Emit
{
    public class DynamicGenericTypeParameterBuilder
    {
        public DynamicGenericTypeParameterBuilder(GenericTypeParameter par, GenericTypeParameterBuilder builder)
        {
            if (par.BaseType != null && par.BaseType != typeof(object))
            {
                builder.SetBaseTypeConstraint(par.BaseType);
            }

            if (par.ConstraintTypes != null)
            {
                builder.SetInterfaceConstraints(par.ConstraintTypes);
            }

            builder.SetGenericParameterAttributes(par.Attribute);

            GenerateTypeParameterBuilder = builder;
        }

        /// <summary>
        /// 获取当前的 <see cref="GenericTypeParameterBuilder"/>。
        /// </summary>
        /// <returns></returns>
        public GenericTypeParameterBuilder GenerateTypeParameterBuilder { get; }
    }
}
