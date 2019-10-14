// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ExpressionVisitor = Fireasy.Common.Linq.Expressions.ExpressionVisitor;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 一个抽象类，动态构造器。
    /// </summary>
    public abstract class DynamicBuilder
    {
        public DynamicBuilder()
        {
        }

        public DynamicBuilder(VisualDecoration visual, CallingDecoration calling)
        {
            Visual = visual;
            Calling = calling;
        }

        public VisualDecoration Visual { get; private set; }

        public CallingDecoration Calling { get; private set; }

        /// <summary>
        /// 获取或设置构造器的上下文对象。
        /// </summary>
        public BuildContext Context { get; protected set; }

        /// <summary>
        /// 使用自定义特性生成器设置此程序集的自定义特性。
        /// </summary>
        /// <typeparam name="T">自定义属性的类型。</typeparam>
        /// <param name="constructorArgs">自定义属性的构造函数的参数。</param>
        public void SetCustomAttribute<T>(params object[] constructorArgs) where T : Attribute
        {
            var types = constructorArgs == null ? new Type[0] : (from s in constructorArgs select s.GetType()).ToArray();
            var con = typeof(T).GetConstructor(types);
            SetCustomAttribute(new CustomAttributeBuilder(con, constructorArgs));
        }

        public void SetCustomAttribute(Expression<Func<Attribute>> expression)
        {
            SetCustomAttribute(CustomAttributeConstructorVisitor.Build(expression));
        }

        /// <summary>
        /// 使用自定义特性生成器设置此程序集的自定义特性。
        /// </summary>
        /// <param name="expression">一个 <see cref="Attribute"/> 的构造表达式。</param>
        public void SetCustomAttribute(Expression expression)
        {
            SetCustomAttribute(CustomAttributeConstructorVisitor.Build(expression));
        }

        /// <summary>
        /// 设置一个 <see cref="CustomAttributeBuilder"/> 对象到当前实例中。
        /// </summary>
        /// <param name="customBuilder">一个 <see cref="CustomAttributeBuilder"/> 对象。</param>
        protected abstract void SetCustomAttribute(CustomAttributeBuilder customBuilder);

        private class CustomAttributeConstructorVisitor : ExpressionVisitor
        {
            private ConstructorInfo constructor;
            private readonly List<object> constructorArgs = new List<object>();
            private readonly List<PropertyInfo> properties = new List<PropertyInfo>();
            private readonly List<object> values = new List<object>();
            private VisitType flags = VisitType.None;

            private enum VisitType
            {
                None,
                ConstructorArgument,
                MemberInit
            }

            public static CustomAttributeBuilder Build(Expression expression)
            {
                var s = new CustomAttributeConstructorVisitor();
                s.Visit(expression);
                if (s.properties.Count != s.values.Count)
                {
                    throw new Exception(SR.GetString(SRKind.PropertyValueLengthNotMatch));
                }

                return new CustomAttributeBuilder(
                    s.constructor, 
                    s.constructorArgs.ToArray(), 
                    s.properties.ToArray(), 
                    s.values.ToArray());
            }

            protected override Expression VisitMember(MemberExpression memberExp)
            {
                //值或引用
                var exp = (Expression)memberExp;
                if (memberExp.Type.IsValueType)
                {
                    exp = Expression.Convert(memberExp, typeof(object));
                }

                var lambda = Expression.Lambda<Func<object>>(exp);
                var fn = lambda.Compile();

                //转换为常量表达式
                return Visit(Expression.Constant(fn(), memberExp.Type));
            }

            protected override Expression VisitMemberInit(MemberInitExpression memberInitExp)
            {
                flags = VisitType.MemberInit;
                var count = memberInitExp.Bindings.Count;
                for (var i = 0; i < count; i++ )
                {
                    var pro = memberInitExp.Bindings[i].Member as PropertyInfo;
                    if (pro == null)
                    {
                        continue;
                    }

                    properties.Add(pro);

                    switch (memberInitExp.Bindings[i].BindingType)
                    {
                        case MemberBindingType.Assignment:
                            var assign = memberInitExp.Bindings[i] as MemberAssignment;
                            if (assign != null && CheckArgumentExpression(assign.Expression))
                            {
                                Visit(assign.Expression);
                            }

                            break;
                    }
                }

                Visit(memberInitExp.NewExpression);
                return memberInitExp;
            }

            protected override Expression VisitNew(NewExpression newExp)
            {
                flags = VisitType.ConstructorArgument;
                constructor = newExp.Constructor;
                foreach (var arg in newExp.Arguments)
                {
                    if (CheckArgumentExpression(arg))
                    {
                        Visit(arg);
                    }
                }

                return newExp;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (flags == VisitType.MemberInit)
                {
                    values.Add(constExp.Value);
                }
                else if (flags == VisitType.ConstructorArgument)
                {
                    constructorArgs.Add(constExp.Value);
                }

                return constExp;
            }

            /// <summary>
            /// 检查表达式能否被正确解析，只有构造里的参数以及属性初始化表达式可以使用。
            /// </summary>
            /// <param name="expression">要检查的表达式。</param>
            /// <returns>能够被解析，则为 true。</returns>
            private bool CheckArgumentExpression(Expression expression)
            {
                if (expression.NodeType == ExpressionType.Constant || 
                    expression.NodeType == ExpressionType.MemberAccess)
                {
                    return true;
                }

                throw new ArgumentException(SR.GetString(SRKind.NotNewExpression, expression));
            }
        }
    }
}
