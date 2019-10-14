#if NETSTANDARD && !NETSTANDARD2_1
namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        public MetadataTypeAttribute(Type metadataClassType)
        {
            MetadataClassType = metadataClassType;
        }

        public Type MetadataClassType { get; private set; }
    }
}
#endif
