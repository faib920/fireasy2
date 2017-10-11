using System;

namespace Fireasy.Common.Composition
{
    /// <summary>
    /// 定义服务接口默认导入的类型。
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class DefaultImportAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="DefaultImportAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="importType">导入类的类型。</param>
        public DefaultImportAttribute(Type importType)
        {
            ImportType = importType;
        }

        /// <summary>
        /// 初始化 <see cref="DefaultImportAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="typeName">类型的名称。</param>
        public DefaultImportAttribute(string typeName)
            : this(Type.GetType(typeName))
        {
        }

        /// <summary>
        /// 获取或设置导入的类型。
        /// </summary>
        public Type ImportType { get; set; }
    }
}
