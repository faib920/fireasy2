// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace Fireasy.Common.Linq.Expressions
{
    /// <summary>
    /// 提供对 <see cref="Expression"/> 对象中各节点的访问方法。
    /// </summary>
    public abstract class ExpressionVisitor
        : System.Linq.Expressions.ExpressionVisitor
    {
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return VisitLambda((LambdaExpression)node);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            var initializers = VisitElementInitializerList(node.Initializers);
            return node.Update(node.Member, initializers);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            var bindings = VisitBindingList(node.Bindings);
            return node.Update(node.Member, bindings);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var args = VisitMemberAndExpressionList(node.Arguments, node.Members);
            return node.Update(node.Constructor, args, node.Members);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var n = (NewExpression)VisitNew(node.NewExpression);
            var bindings = VisitBindingList(node.Bindings);
            return node.Update(n, bindings);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            var n = (NewExpression)VisitNew(node.NewExpression);
            var initializers = VisitElementInitializerList(node.Initializers);
            return node.Update(n, initializers);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var exprs = VisitMemberAndExpressionList(node.Expressions);
            return node.Update(node.Type, exprs);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            var args = VisitMemberAndExpressionList(node.Arguments);
            var expr = Visit(node.Expression);
            return node.Update(expr, args);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = Visit(node.Object);
            var args = VisitExpressionList(node.Arguments);
            return node.Update(obj, node.Method, args);
        }

        protected override Expression VisitConstant(ConstantExpression constExp)
        {
            if (constExp.Value is IQueryable queryable)
            {
                Visit(queryable.Expression);
            }

            return constExp;
        }

        /// <summary>
        /// 访问 <see cref="LambdaExpression"/>。
        /// </summary>
        /// <param name="lambdaExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitLambda(LambdaExpression lambdaExp)
        {
            var eleType = lambdaExp.Type;
            var defType = lambdaExp.Type.GetGenericTypeDefinition();
            var genTypes = lambdaExp.Type.GetGenericArguments();
            var parameters = lambdaExp.Parameters.Select(s => (ParameterExpression)Visit(s)).ToArray();

            var typeChanged = false;

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == lambdaExp.Parameters[i])
                {
                    continue;
                }

                for (var j = 0; j < genTypes.Length; j++)
                {
                    if (genTypes[j] == lambdaExp.Parameters[i].Type)
                    {
                        genTypes[j] = parameters[i].Type;
                        typeChanged = true;
                    }
                }
            }

            if (typeChanged)
            {
                eleType = defType.MakeGenericType(genTypes);
            }

            var body = Visit(lambdaExp.Body);
            return lambdaExp.Update(eleType, body, parameters);
        }

        /// <summary>
        /// 访问 <see cref="MemberBinding"/>。
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(SR.GetString(SRKind.UnableProcessExpression, binding.BindingType));
            }
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original != null)
            {
                List<Expression> list = null;
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    Expression p = this.Visit(original[i]);
                    if (list != null)
                    {
                        list.Add(p);
                    }
                    else if (p != original[i])
                    {
                        list = new List<Expression>(n);
                        for (int j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }
                        list.Add(p);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
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
        protected virtual ReadOnlyCollection<Expression> VisitMemberAndExpressionList(ReadOnlyCollection<Expression> original, ReadOnlyCollection<MemberInfo> members = null)
        {
            if (original != null)
            {
                List<Expression> list = null;
                var count = original.Count;
                for (int i = 0, n = count; i < n; i++)
                {
                    var p = VisitMemberAndExpression(members != null ? members[i] : null, original[i]);
                    if (list != null)
                    {
                        list.Add(p);
                    }
                    else if (p != original[i])
                    {
                        list = new List<Expression>(n);
                        for (var j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }
                        list.Add(p);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
                }
            }
            return original;
        }

        protected virtual Expression VisitMemberAndExpression(MemberInfo member, Expression expression)
        {
            return this.Visit(expression);
        }

        /// <summary>
        /// 访问成员绑定集合。
        /// </summary>
        /// <param name="original">成员绑定集合。</param>
        /// <returns></returns>
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            var count = original.Count;
            for (int i = 0, n = count; i < n; i++)
            {
                var b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
            {
                return list;
            }
            return original;
        }

        /// <summary>
        /// 访问 <see cref="ElementInit"/>。
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            var arguments = VisitMemberAndExpressionList(initializer.Arguments);
            return arguments != initializer.Arguments ? Expression.ElementInit(initializer.AddMethod, arguments) : initializer;
        }

        /// <summary>
        /// 访问元素初始值集合。
        /// </summary>
        /// <param name="original">元素初始值集合。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            var count = original.Count;
            for (int i = 0, n = count; i < n; i++)
            {
                var init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
            {
                return list;
            }
            return original;
        }

        /// <summary>
        /// 访问未知类型的表达式。
        /// </summary>
        /// <param name="expression">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitUnknown(Expression expression)
        {
            throw new Exception(SR.GetString(SRKind.UnableProcessExpression, expression.NodeType));
        }
    }
}
