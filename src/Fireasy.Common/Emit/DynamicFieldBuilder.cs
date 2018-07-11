// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 用于创建一个字段。
    /// </summary>
    public class DynamicFieldBuilder : DynamicBuilder
    {
        private FieldBuilder fieldBuilder;
        private readonly object defaultValue;
        private readonly FieldAttributes attributes;

        internal DynamicFieldBuilder(BuildContext context, string fieldName, Type fieldType, object defaultValue = null, VisualDecoration visual = VisualDecoration.Private, CallingDecoration calling = CallingDecoration.Standard)
            : base(visual, calling)
        {
            FieldName = fieldName;
            FieldType = fieldType;
            attributes = GetAttributes(visual, calling);
            this.defaultValue = defaultValue;
            Context = context;
            InitBuilder();
        }

        /// <summary>
        /// 获取字段的名称。
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// 获取字段的类型。
        /// </summary>
        public Type FieldType { get; private set; }

        /// <summary>
        /// 设置一个 <see cref="CustomAttributeBuilder"/> 对象到当前实例关联的 <see cref="FieldBuilder"/> 对象。
        /// </summary>
        /// <param name="customBuilder">一个 <see cref="CustomAttributeBuilder"/> 对象。</param>
        protected override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            fieldBuilder.SetCustomAttribute(customBuilder);
        }

        /// <summary>
        /// 获取 <see cref="FieldBuilder"/> 对象。
        /// </summary>
        /// <returns></returns>
        public FieldBuilder FieldBuilder
        {
            get { return fieldBuilder; }
        }

        private FieldAttributes GetAttributes(VisualDecoration visual, CallingDecoration calling)
        {
            var attrs = FieldAttributes.HasDefault;
            switch (calling)
            {
                case CallingDecoration.Static:
                    attrs |= FieldAttributes.Static;
                    break;
            }

            switch (visual)
            {
                case VisualDecoration.Internal:
                    attrs |= FieldAttributes.Assembly;
                    break;
                case VisualDecoration.Private:
                    attrs |= FieldAttributes.Private;
                    break;
                case VisualDecoration.Public:
                    attrs |= FieldAttributes.Public;
                    break;
            }

            return attrs;
        }

        private void InitBuilder()
        {
            if (Context.EnumBuilder == null)
            {
                fieldBuilder = Context.TypeBuilder.TypeBuilder.DefineField(FieldName, FieldType, attributes);
                if (defaultValue != null)
                {
                    fieldBuilder.SetConstant(defaultValue);
                }
            }
            else
            {
                fieldBuilder = Context.EnumBuilder.EnumBuilder.DefineLiteral(FieldName, defaultValue);
            }
        }
    }
}
