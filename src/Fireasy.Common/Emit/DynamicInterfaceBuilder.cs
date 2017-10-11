// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Reflection;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 用于创建一个动态的接口。
    /// </summary>
    public class DynamicInterfaceBuilder : DynamicTypeBuilder
    {
        internal DynamicInterfaceBuilder(BuildContext context, string typeName, VisualDecoration visual) :
            base(context, typeName, visual, CallingDecoration.Standard, null)
        {
        }

        /// <summary>
        /// 获取或设置动态类型所继承的类型。
        /// </summary>
        public override Type BaseType
        {
            get
            {
                return null;
            }

            set
            {
                throw new ArgumentException(SR.GetString(SRKind.InterfaceNoBaseType));
            }
        }

        /// <summary>
        /// 获取 <see cref="TypeAttributes"/>。
        /// </summary>
        /// <returns></returns>
        protected override TypeAttributes GetTypeAttributes()
        {
            return TypeAttributes.Interface | TypeAttributes.Abstract;
        }

        internal override PropertyAttributes GetPropertyAttributes()
        {
            return PropertyAttributes.HasDefault;
        }

        internal override MethodAttributes GetMethodAttributes()
        {
            return MethodAttributes.Virtual | MethodAttributes.Abstract;
        }

        /// <summary>
        /// 定义一个字段。
        /// </summary>
        /// <param name="fieldName">字段的名称。</param>
        /// <param name="fieldType">字段的类型。</param>
        /// <param name="defaultValue"></param>
        /// <param name="visual"></param>
        /// <param name="calling"></param>
        /// <returns></returns>
        public override DynamicFieldBuilder DefineField(string fieldName, Type fieldType, object defaultValue = null, VisualDecoration visual = VisualDecoration.Private, CallingDecoration calling = CallingDecoration.Standard)
        {
            return null;
        }
    }
}
