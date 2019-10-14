// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// <see cref="Expression"/> 的写入器。
    /// </summary>
    public class ExpressionJsonWriter
    {
        private Expression expression;
        private JsonSerializer serializer;

        /// <summary>
        /// 获取类型与参数名称的字典。
        /// </summary>
        protected readonly Dictionary<Type, string> TypeDictionary = new Dictionary<Type, string>();

        /// <summary>
        /// 初始化 <ExpressionJsonWriter>类的新实例。</ExpressionJsonWriter>
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="expression"></param>
        public ExpressionJsonWriter(JsonSerializer serializer, Expression expression)
        {
            this.serializer = serializer;
            this.expression = expression;
        }

        /// <summary>
        /// 获取 <see cref="JsonWriter"/> 对象。
        /// </summary>
        protected JsonWriter JsonWriter { get; private set; }

        /// <summary>
        /// 分段写表达式块。
        /// </summary>
        /// <param name="expression"></param>
        private void WriteSegment(Expression expression)
        {
            if (expression == null)
            {
                JsonWriter.WriteNull();
                return;
            }

            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey(expression.NodeType.ToString());

            WriteExpression(expression);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 将 <paramref name="expression"/> 写成字符串。
        /// </summary>
        /// <param name="expression"></param>
        protected void WriteExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    WriteLambdaExpression((LambdaExpression)expression);
                    break;
                case ExpressionType.Parameter:
                    WriteParameterExpression((ParameterExpression)expression);
                    break;
                case ExpressionType.Constant:
                    WriteConstantExpression((ConstantExpression)expression);
                    break;
                case ExpressionType.MemberAccess:
                    WriteMemberExpression((MemberExpression)expression);
                    break;
                case ExpressionType.Call:
                    WriteMethodCallExpression((MethodCallExpression)expression);
                    break;
                case ExpressionType.New:
                    WriteNewExpression((NewExpression)expression);
                    break;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    WriteNewArrayExpression((NewArrayExpression)expression);
                    break;
                case ExpressionType.MemberInit:
                    WriteMemberInitExpression((MemberInitExpression)expression);
                    break;
                case ExpressionType.Conditional:
                    WriteConditionalExpression((ConditionalExpression)expression);
                    break;
                case ExpressionType.ListInit:
                    WriteListInitExpression((ListInitExpression)expression);
                    break;
                case ExpressionType.TypeIs:
                case ExpressionType.TypeEqual:
                    WriteTypeBinaryExpression((TypeBinaryExpression)expression);
                    break;
                case ExpressionType.Invoke:
                    WriteInvocationExpression((InvocationExpression)expression);
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    WriteUnaryExpression((UnaryExpression)expression);
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                    WriteBinaryExpression((BinaryExpression)expression);
                    break;
            }
        }

        /// <summary>
        /// 写 <see cref="ParameterExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteParameterExpression(ParameterExpression node)
        {
            var parName = string.Empty;
            var keyExists = true;
            if (!TypeDictionary.TryGetValue(node.Type, out parName))
            {
                parName = string.IsNullOrEmpty(node.Name) ? node.ToString() : node.Name;
                TypeDictionary.Add(node.Type, parName);
                keyExists = false;
            }

            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("Name");
            JsonWriter.WriteString(parName);

            if (!keyExists)
            {
                JsonWriter.WriteComma();
                JsonWriter.WriteKey("Type");
                JsonWriter.WriteString(WriteTypeName(node.Type));
            }

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="LambdaExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteLambdaExpression(LambdaExpression node)
        {
            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("Parameters");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < node.Parameters.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteSegment(node.Parameters[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Body");

            WriteSegment(node.Body);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="BinaryExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteBinaryExpression(BinaryExpression node)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Left");
            WriteSegment(node.Left);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Right");
            WriteSegment(node.Right);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="MemberExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteMemberExpression(MemberExpression node)
        {
            JsonWriter.WriteStartObject();
            
            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Member.DeclaringType));
            
            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Member");
            JsonWriter.WriteString(node.Member.Name);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Expression");
            WriteSegment(node.Expression);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="MethodCallExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteMethodCallExpression(MethodCallExpression node)
        {
            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Method.DeclaringType));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Method");
            JsonWriter.WriteString(node.Method.Name);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Object");

            if (node.Object == null)
            {
                JsonWriter.WriteNull();
            }
            else
            {
                WriteSegment(node.Object);
            }

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Arguments");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteSegment(node.Arguments[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("GenericArgTypes");
            JsonWriter.WriteStartArray();

            var genericArgTypes = node.Method.GetGenericArguments();
            for (var i = 0; i < genericArgTypes.Length; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                JsonWriter.WriteString(WriteTypeName(genericArgTypes[i]));
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("ParameterTypes");
            JsonWriter.WriteStartArray();

            var parameterTypes = node.Method.GetParameters().Select(s => s.ParameterType).ToArray();
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                JsonWriter.WriteString(WriteTypeName(parameterTypes[i]));
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="UnaryExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteUnaryExpression(UnaryExpression node)
        {
            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Type));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Operand");

            WriteSegment(node.Operand);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="ConstantExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteConstantExpression(ConstantExpression node)
        {
            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Type));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Value");

            JsonWriter.WriteString(serializer.Serialize(node.Value));

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="NewExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteNewExpression(NewExpression node)
        {
            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Constructor.DeclaringType));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("ParameterTypes");
            JsonWriter.WriteStartArray();

            var parameterTypes = node.Constructor.GetParameters().Select(s => s.ParameterType).ToArray();
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                JsonWriter.WriteString(WriteTypeName(parameterTypes[i]));
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Members");
            JsonWriter.WriteStartArray();

            if (node.Members != null)
            {
                for (var i = 0; i < node.Members.Count; i++)
                {
                    if (i > 0)
                    {
                        JsonWriter.WriteComma();
                    }

                    JsonWriter.WriteStartObject();

                    JsonWriter.WriteKey("Type");
                    JsonWriter.WriteString(WriteTypeName(node.Members[i].DeclaringType));

                    JsonWriter.WriteComma();
                    JsonWriter.WriteKey("Name");
                    JsonWriter.WriteString(node.Members[i].Name);

                    JsonWriter.WriteEndObject();
                }
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Arguments");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteSegment(node.Arguments[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="NewArrayExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteNewArrayExpression(NewArrayExpression node)
        {
            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Type));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Initializers");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < node.Expressions.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteSegment(node.Expressions[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="MemberInitExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteMemberInitExpression(MemberInitExpression node)
        {
            JsonWriter.WriteStartObject();
            JsonWriter.WriteKey("New");
            WriteSegment(node.NewExpression);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Bindings");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < node.Bindings.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteMemberBinding(node.Bindings[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="MemberBinding"/> 对象。
        /// </summary>
        /// <param name="binding"></param>
        protected virtual void WriteMemberBinding(MemberBinding binding)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey(binding.BindingType.ToString());

            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    WriteMemberAssignment((MemberAssignment)binding);
                    break;
                case MemberBindingType.MemberBinding:
                    WriteMemberMemberBinding((MemberMemberBinding)binding);
                    break;
                case MemberBindingType.ListBinding:
                    WriteMemberListBinding((MemberListBinding)binding);
                    break;
            }

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="MemberMemberBinding"/> 对象。
        /// </summary>
        /// <param name="binding"></param>
        protected virtual void WriteMemberMemberBinding(MemberMemberBinding binding)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(binding.Member.DeclaringType));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Name");
            JsonWriter.WriteString(binding.Member.Name);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Bindings");
            JsonWriter.WriteStartArray();

            if (binding.Bindings != null)
            {
                for (var i = 0; i < binding.Bindings.Count; i++)
                {
                    if (i > 0)
                    {
                        JsonWriter.WriteComma();
                    }

                    WriteMemberBinding(binding.Bindings[i]);
                }
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="MemberAssignment"/> 对象。
        /// </summary>
        /// <param name="assign"></param>
        protected virtual void WriteMemberAssignment(MemberAssignment assign)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(assign.Member.DeclaringType));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Name");
            JsonWriter.WriteString(assign.Member.Name);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Expression");
            WriteSegment(assign.Expression);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="MemberListBinding"/> 对象。
        /// </summary>
        /// <param name="binding"></param>
        protected virtual void WriteMemberListBinding(MemberListBinding binding)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(binding.Member.DeclaringType));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Name");
            JsonWriter.WriteString(binding.Member.Name);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Initializers");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < binding.Initializers.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteElementInitializer(binding.Initializers[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="ElementInit"/> 对象。
        /// </summary>
        /// <param name="initializer"></param>
        protected virtual void WriteElementInitializer(ElementInit initializer)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Arguments");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < initializer.Arguments.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteSegment(initializer.Arguments[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="ConditionalExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteConditionalExpression(ConditionalExpression node)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Type));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("True");
            WriteSegment(node.IfTrue);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("False");
            WriteSegment(node.IfFalse);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Test");
            WriteSegment(node.Test);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="ListInitExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteListInitExpression(ListInitExpression node)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("New");
            WriteSegment(node.NewExpression);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Initializers");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < node.Initializers.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteElementInitializer(node.Initializers[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="TypeBinaryExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteTypeBinaryExpression(TypeBinaryExpression node)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Type");
            JsonWriter.WriteString(WriteTypeName(node.Type));

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Expression");
            WriteSegment(node.Expression);

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 写 <see cref="InvocationExpression"/> 对象。
        /// </summary>
        /// <param name="node"></param>
        protected virtual void WriteInvocationExpression(InvocationExpression node)
        {
            JsonWriter.WriteStartObject();

            JsonWriter.WriteKey("Expression");
            WriteSegment(node.Expression);

            JsonWriter.WriteComma();
            JsonWriter.WriteKey("Arguments");
            JsonWriter.WriteStartArray();

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                if (i > 0)
                {
                    JsonWriter.WriteComma();
                }

                WriteSegment(node.Arguments[i]);
            }

            JsonWriter.WriteEndArray();

            JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// 将类型转换为字符串。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string WriteTypeName(Type type)
        {
            return type.Assembly.GetName().Name == "mscorlib" ? type.FullName : type.AssemblyQualifiedName;
        }

        public override string ToString()
        {
            using (var sw = new StringWriter())
            using (JsonWriter = new JsonWriter(sw))
            {
                expression = PartialEvaluator.Eval(expression);
                WriteSegment(expression);
                return sw.ToString();
            }
        }
    }
}
