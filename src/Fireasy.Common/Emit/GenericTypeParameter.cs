using System;
using System.Reflection;

namespace Fireasy.Common.Emit
{
    public class GenericTypeParameter
    {
        public GenericTypeParameter(string name, Type baseType = null, GenericParameterAttributes attr = GenericParameterAttributes.None, params Type[] contraintTypes)
        {
            Name = name;
            BaseType = baseType;
            Attribute = attr;
            ConstraintTypes = contraintTypes;
        }

        public static GenericTypeParameter From(Type type)
        {
            return new GenericTypeParameter(type.Name, type.BaseType, type.GenericParameterAttributes, type.GetGenericParameterConstraints());
        }

        public string Name { get; set; }

        public Type BaseType { get; set; }

        public Type[] ConstraintTypes { get; set; }

        public GenericParameterAttributes Attribute { get; set; }
    }
}
