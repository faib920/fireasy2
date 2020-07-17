// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fireasy.Common.Linq.Expressions
{
    /// <summary>
    /// 用于将表达式树使用字符串表示。
    /// </summary>
    public class ExpressionWriter : ExpressionVisitor
    {
        private readonly TextWriter _writer;
        private int _depth;
        private static readonly char[] _splitters = new char[] { '\n', '\r' };

        /// <summary>
        /// 初始化 <see cref="ExpressionWriter"/> 类的新实例。
        /// </summary>
        /// <param name="writer"></param>
        protected ExpressionWriter(TextWriter writer)
        {
            _writer = writer;
            IndentationWidth = 2;
        }

        /// <summary>
        /// 将表达式树写入到指定的 <see cref="TextWriter"/> 对象中。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="expression"></param>
        public static void Write(TextWriter writer, Expression expression)
        {
            new ExpressionWriter(writer).Visit(expression);
        }

        /// <summary>
        /// 将表达式树转换为字符串表示。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string WriteToString(Expression expression)
        {
            using var sw = new StringWriter();
            Write(sw, expression);
            return sw.ToString();
        }

        /// <summary>
        /// 语法的缩进方式。
        /// </summary>
        protected enum Indentation
        {
            /// <summary>
            /// 以前一个缩进保持一致。
            /// </summary>
            Same,

            /// <summary>
            /// 内缩进。
            /// </summary>
            Inner,

            /// <summary>
            /// 外缩进。
            /// </summary>
            Outer
        }

        /// <summary>
        /// 获取或设置缩进的字符宽度。
        /// </summary>
        protected int IndentationWidth { get; set; }

        /// <summary>
        /// 向编码器中写入一个空行。
        /// </summary>
        /// <param name="style">缩进方式。</param>
        protected void WriteLine(Indentation style)
        {
            _writer.WriteLine();
            Indent(style);
            for (int i = 0, n = _depth * IndentationWidth; i < n; i++)
            {
                _writer.Write(" ");
            }
        }

        /// <summary>
        /// 向编码器中写入一个字符串。
        /// </summary>
        /// <param name="text">字符串。</param>
        protected void Write(string text)
        {
            if (text.IndexOf('\n') >= 0)
            {
                string[] lines = text.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0, n = lines.Length; i < n; i++)
                {
                    Write(lines[i]);
                    if (i < n - 1)
                    {
                        WriteLine(Indentation.Same);
                    }
                }
            }
            else
            {
                _writer.Write(text);
            }
        }

        /// <summary>
        /// 控制编码器缩进。
        /// </summary>
        /// <param name="style">缩进方式。</param>
        protected void Indent(Indentation style)
        {
            if (style == Indentation.Inner)
            {
                _depth++;
            }
            else if (style == Indentation.Outer)
            {
                _depth--;
                System.Diagnostics.Debug.Assert(_depth >= 0);
            }
        }

        /// <summary>
        /// 获取指定运算的操作符。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Not:
                    return "!";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "&&";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "||";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Coalesce:
                    return "??";
                case ExpressionType.RightShift:
                    return ">>";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.ExclusiveOr:
                    return "^";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 访问 <see cref="BinaryExpression"/> 表达式。
        /// </summary>
        /// <param name="b"><see cref="BinaryExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    Visit(b.Left);
                    Write("[");
                    Visit(b.Right);
                    Write("]");
                    break;
                case ExpressionType.Power:
                    Write("POW(");
                    Visit(b.Left);
                    Write(", ");
                    Visit(b.Right);
                    Write(")");
                    break;
                default:
                    Visit(b.Left);
                    Write(" ");
                    Write(GetOperator(b.NodeType));
                    Write(" ");
                    Visit(b.Right);
                    break;
            }
            return b;
        }

        /// <summary>
        /// 访问 <see cref="UnaryExpression"/> 表达式。
        /// </summary>
        /// <param name="u"><see cref="UnaryExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    Write("((");
                    Write(GetTypeName(u.Type));
                    Write(")");
                    Visit(u.Operand);
                    Write(")");
                    break;
                case ExpressionType.ArrayLength:
                    Visit(u.Operand);
                    Write(".Length");
                    break;
                case ExpressionType.Quote:
                    Visit(u.Operand);
                    break;
                case ExpressionType.TypeAs:
                    Visit(u.Operand);
                    Write(" as ");
                    Write(GetTypeName(u.Type));
                    break;
                case ExpressionType.UnaryPlus:
                    Visit(u.Operand);
                    break;
                default:
                    Write(GetOperator(u.NodeType));
                    Visit(u.Operand);
                    break;
            }
            return u;
        }

        /// <summary>
        /// 获取表示泛型类型或匿名类型的名称的字符串。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual string GetTypeName(Type type)
        {
            string name = type.Name;
            name = name.Replace('+', '.');
            int iGeneneric = name.IndexOf('`');
            if (iGeneneric > 0)
            {
                name = name.Substring(0, iGeneneric);
            }

            if (type.IsGenericType || type.IsGenericTypeDefinition)
            {
                var sb = new StringBuilder();
                sb.Append(name);
                sb.Append("<");

                var args = type.GetGenericArguments();
                for (int i = 0, n = args.Length; i < n; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    if (type.IsGenericType)
                    {
                        sb.Append(GetTypeName(args[i]));
                    }
                }

                sb.Append(">");
                name = sb.ToString();
            }

            return name;
        }

        /// <summary>
        /// 访问 <see cref="ConditionalExpression"/> 表达式。
        /// </summary>
        /// <param name="c"><see cref="ConditionalExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitConditional(ConditionalExpression c)
        {
            Visit(c.Test);
            WriteLine(Indentation.Inner);
            Write("? ");
            Visit(c.IfTrue);
            WriteLine(Indentation.Same);
            Write(": ");
            Visit(c.IfFalse);
            Indent(Indentation.Outer);
            return c;
        }

        /// <summary>
        /// 访问成员绑定集合。
        /// </summary>
        /// <param name="original">成员绑定集合。</param>
        /// <returns></returns>
        protected override IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            for (int i = 0, n = original.Count; i < n; i++)
            {
                VisitBinding(original[i]);
                if (i < n - 1)
                {
                    Write(",");
                    WriteLine(Indentation.Same);
                }
            }

            return original;
        }

        private static readonly char[] special = new[] { '\n', '\n', '\\' };

        /// <summary>
        /// 访问 <see cref="ConstantExpression"/> 表达式。
        /// </summary>
        /// <param name="c"><see cref="ConstantExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
            {
                Write("null");
            }
            else if (c.Type == typeof(string))
            {
                string value = c.Value.ToString();
                if (value.IndexOfAny(special) >= 0)
                {
                    Write("@");
                }

                Write($"\"{c.Value}\"");
            }
            else if (c.Type == typeof(DateTime))
            {
                Write($"new DateTime(\"{c.Value}\")");
            }
            else if (typeof(IQueryable).IsAssignableFrom(c.Type))
            {
                var elementType = c.Type.GetEnumerableElementType();
                Write($"Queryable<{elementType}>(");
                Visit(((IQueryable)c.Value).Expression);
                Write(")");
            }
            else if (c.Type.IsArray || typeof(IEnumerable).IsAssignableFrom(c.Type))
            {
                var elementType = c.Type.GetEnumerableElementType() ?? typeof(object);
                VisitNewArray(
                    Expression.NewArrayInit(
                        elementType,
                        ((IEnumerable)c.Value).OfType<object>().Select(v => (Expression)Expression.Constant(v, elementType))
                        ));
            }
            else if (c.Value.ToString() != c.Type.Name)
            {
                Write(c.Value.ToString());
            }
            else
            {
                Write($"new {c.Type} ");
                Write("{ ");
                var assert = new AssertFlag();
                foreach (var p in c.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (c.Value is ILazyManager lazyMgr && !lazyMgr.IsValueCreated(p.Name))
                    {
                        continue;
                    }

                    var value = p.GetValue(c.Value, null);
                    if (value == null)
                    {
                        continue;
                    }

                    if (!assert.AssertTrue())
                    {
                        Write(", ");
                    }

                    Write(p.Name + " = ");
                    if (p.PropertyType.IsStringOrDateTime())
                    {
                        Write($"\"{value}\"");
                    }
                    else
                    {
                        Write(value.ToString());
                    }
                }

                Write(" }");
            }

            return c;
        }

        /// <summary>
        /// 访问 <see cref="ElementInit"/>。
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        protected override ElementInit VisitElementInitializer(ElementInit initializer)
        {
            if (initializer.Arguments.Count > 1)
            {
                Write("{");

                for (int i = 0, n = initializer.Arguments.Count; i < n; i++)
                {
                    Visit(initializer.Arguments[i]);
                    if (i < n - 1)
                    {
                        Write(", ");
                    }
                }

                Write("}");
            }
            else
            {
                Visit(initializer.Arguments[0]);
            }

            return initializer;
        }

        /// <summary>
        /// 访问元素初始值集合。
        /// </summary>
        /// <param name="original">元素初始值集合。</param>
        /// <returns></returns>
        protected override IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            for (int i = 0, n = original.Count; i < n; i++)
            {
                VisitElementInitializer(original[i]);

                if (i < n - 1)
                {
                    Write(",");
                    WriteLine(Indentation.Same);
                }
            }

            return original;
        }

        /// <summary>
        /// 访问表达式列表。
        /// </summary>
        /// <param name="original"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        protected override ReadOnlyCollection<Expression> VisitMemberAndExpressionList(ReadOnlyCollection<Expression> original, ReadOnlyCollection<MemberInfo> members = null)
        {
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Visit(original[i]);

                if (i < n - 1)
                {
                    Write(",");
                    WriteLine(Indentation.Same);
                }
            }

            return original;
        }

        /// <summary>
        /// 访问 <see cref="InvocationExpression"/>。
        /// </summary>
        /// <param name="iv">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            Write("Invoke(");
            WriteLine(Indentation.Inner);
            VisitMemberAndExpressionList(iv.Arguments);
            Write(", ");
            WriteLine(Indentation.Same);
            Visit(iv.Expression);
            WriteLine(Indentation.Same);
            Write(")");
            Indent(Indentation.Outer);
            return iv;
        }

        /// <summary>
        /// 访问 <see cref="LambdaExpression"/>。
        /// </summary>
        /// <param name="lambda">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Parameters.Count != 1)
            {
                Write("(");

                for (int i = 0, n = lambda.Parameters.Count; i < n; i++)
                {
                    Write(lambda.Parameters[i].Name);
                    if (i < n - 1)
                    {
                        Write(", ");
                    }
                }

                Write(")");
            }
            else
            {
                Write(lambda.Parameters[0].Name);
            }

            Write(" => ");
            Visit(lambda.Body);

            return lambda;
        }

        /// <summary>
        /// 访问 <see cref="ListInitExpression"/> 表达式。
        /// </summary>
        /// <param name="init"><see cref="ListInitExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitListInit(ListInitExpression init)
        {
            Visit(init.NewExpression);
            Write(" {");
            WriteLine(Indentation.Inner);
            VisitElementInitializerList(init.Initializers);
            WriteLine(Indentation.Outer);
            Write("}");
            return init;
        }

        /// <summary>
        /// 访问 <see cref="MemberExpression"/> 表达式。
        /// </summary>
        /// <param name="m"><see cref="MemberExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression m)
        {
            Visit(m.Expression);
            Write(".");
            Write(m.Member.Name);

            return m;
        }

        /// <summary>
        /// 访问 <see cref="MemberAssignment"/> 表达式。。
        /// </summary>
        /// <param name="assignment"><see cref="MemberAssignment"/> 表达式。</param>
        /// <returns></returns>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Write(assignment.Member.Name);
            Write(" = ");
            Visit(assignment.Expression);
            return assignment;
        }

        /// <summary>
        /// 访问 <see cref="MemberInitExpression"/> 表达式。
        /// </summary>
        /// <param name="init"><see cref="MemberInitExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitMemberInit(MemberInitExpression init)
        {
            Visit(init.NewExpression);
            Write(" {");
            WriteLine(Indentation.Inner);
            VisitBindingList(init.Bindings);
            WriteLine(Indentation.Outer);
            Write("}");
            return init;
        }

        /// <summary>
        /// 访问 <see cref="MemberListBinding"/> 表达式。
        /// </summary>
        /// <param name="binding"><see cref="MemberListBinding"/> 表达式。</param>
        /// <returns></returns>
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            Write(binding.Member.Name);
            Write(" = {");
            WriteLine(Indentation.Inner);
            VisitElementInitializerList(binding.Initializers);
            WriteLine(Indentation.Outer);
            Write("}");
            return binding;
        }

        /// <summary>
        /// 访问 <see cref="MemberMemberBinding"/> 表达式。
        /// </summary>
        /// <param name="binding"><see cref="MemberMemberBinding"/> 表达式。</param>
        /// <returns></returns>
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            Write(binding.Member.Name);
            Write(" = {");
            WriteLine(Indentation.Inner);
            VisitBindingList(binding.Bindings);
            WriteLine(Indentation.Outer);
            Write("}");
            return binding;
        }

        /// <summary>
        /// 访问 <see cref="MethodCallExpression"/> 表达式。
        /// </summary>
        /// <param name="m"><see cref="MethodCallExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Object != null)
            {
                Visit(m.Object);
            }
            else
            {
                Write(GetTypeName(m.Method.DeclaringType));
            }

            Write(".");
            Write(m.Method.Name);
            Write("(");

            if (m.Arguments.Count > 1)
            {
                WriteLine(Indentation.Inner);
            }

            VisitMemberAndExpressionList(m.Arguments);

            if (m.Arguments.Count > 1)
            {
                WriteLine(Indentation.Outer);
            }

            Write(")");

            return m;
        }

        /// <summary>
        /// 访问 <see cref="NewExpression"/> 表达式。
        /// </summary>
        /// <param name="nex"><see cref="NewExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression nex)
        {
            Write("new ");
            Write(GetTypeName(nex.Constructor.DeclaringType));
            Write("(");

            if (nex.Arguments.Count > 1)
            {
                WriteLine(Indentation.Inner);
            }

            VisitMemberAndExpressionList(nex.Arguments);

            if (nex.Arguments.Count > 1)
            {
                WriteLine(Indentation.Outer);
            }

            Write(")");

            return nex;
        }

        /// <summary>
        /// 访问 <see cref="NewArrayExpression"/> 表达式。
        /// </summary>
        /// <param name="na"><see cref="NewArrayExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitNewArray(NewArrayExpression na)
        {
            Write("new ");
            Write(GetTypeName(na.Type.GetEnumerableElementType()));
            Write("[] {");

            if (na.Expressions.Count > 1)
            {
                WriteLine(Indentation.Inner);
            }

            VisitMemberAndExpressionList(na.Expressions);
            if (na.Expressions.Count > 1)
            {
                WriteLine(Indentation.Outer);
            }

            Write("}");

            return na;
        }

        /// <summary>
        /// 访问 <see cref="ParameterExpression"/> 表达式。
        /// </summary>
        /// <param name="p"><see cref="ParameterExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            Write(p.Name);
            return p;
        }

        /// <summary>
        /// 访问 <see cref="TypeBinaryExpression"/> 表达式。
        /// </summary>
        /// <param name="b"><see cref="TypeBinaryExpression"/> 表达式。</param>
        /// <returns></returns>
        protected override Expression VisitTypeBinary(TypeBinaryExpression b)
        {
            Visit(b.Expression);

            Write(" is ");
            Write(GetTypeName(b.TypeOperand));

            return b;
        }
    }
}
