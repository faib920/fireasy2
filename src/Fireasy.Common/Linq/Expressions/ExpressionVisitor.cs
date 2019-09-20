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
#if !NET35
        : System.Linq.Expressions.ExpressionVisitor
#endif
    {
#if NET35
        /// <summary>
        /// 访问 <see cref="Expression"/>。
        /// </summary>
        /// <param name="expression">要访问的表达式。</param>
        /// <returns></returns>
        public virtual Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    return VisitUnary((UnaryExpression)expression);
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
                    return VisitBinary((BinaryExpression)expression);
                case ExpressionType.TypeIs:
                    return VisitTypeBinary((TypeBinaryExpression)expression);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expression);
                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)expression);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)expression);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)expression);
                case ExpressionType.New:
                    return VisitNew((NewExpression)expression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)expression);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)expression);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)expression);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)expression);
                default:
                    return VisitUnknown(expression);
            }
        }

        /// <summary>
        /// 访问 <see cref="UnaryExpression"/>。
        /// </summary>
        /// <param name="unaryExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitUnary(UnaryExpression unaryExp)
        {
            var operand = Visit(unaryExp.Operand);
            return unaryExp.Update(operand, unaryExp.Type, unaryExp.Method);
        }

        /// <summary>
        /// 访问 <see cref="BinaryExpression"/>。
        /// </summary>
        /// <param name="binary">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitBinary(BinaryExpression binary)
        {
            var left = Visit(binary.Left);
            var right = Visit(binary.Right);
            var conversion = Visit(binary.Conversion);
            return binary.Update(left, right, conversion, binary.IsLiftedToNull, binary.Method);
        }

        /// <summary>
        /// 访问 <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="constExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitConstant(ConstantExpression constExp)
        {
            if (constExp.Value is IQueryable queryable)
            {
                Visit(queryable.Expression);
            }

            return constExp;
        }

        /// <summary>
        /// 访问 <see cref="ConditionalExpression"/>。
        /// </summary>
        /// <param name="conditionExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitConditional(ConditionalExpression conditionExp)
        {
            var test = Visit(conditionExp.Test);
            var ifTrue = Visit(conditionExp.IfTrue);
            var ifFalse = Visit(conditionExp.IfFalse);
            return conditionExp.Update(test, ifTrue, ifFalse);
        }

        /// <summary>
        /// 访问 <see cref="ParameterExpression"/>。
        /// </summary>
        /// <param name="paraExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitParameter(ParameterExpression paraExp)
        {
            return paraExp;
        }

        /// <summary>
        /// 访问 <see cref="MemberExpression"/>。
        /// </summary>
        /// <param name="memberExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitMember(MemberExpression memberExp)
        {
            var exp = Visit(memberExp.Expression);
            return memberExp.Update(exp, memberExp.Member);
        }

        /// <summary>
        /// 访问 <see cref="MethodCallExpression"/>。
        /// </summary>
        /// <param name="methodCallExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression methodCallExp)
        {
            var obj = Visit(methodCallExp.Object);
            var args = VisitExpressionList(methodCallExp.Arguments);
            return methodCallExp.Update(obj, methodCallExp.Method, args);
        }

        /// <summary>
        /// 访问 <see cref="MemberAssignment"/>。
        /// </summary>
        /// <param name="assignment"></param>
        /// <returns></returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            var e = Visit(assignment.Expression);
            return assignment.Update(assignment.Member, e);
        }

        /// <summary>
        /// 访问 <see cref="MemberMemberBinding"/>。
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            var bindings = VisitBindingList(binding.Bindings);
            return binding.Update(binding.Member, bindings);
        }

        /// <summary>
        /// 访问 <see cref="MemberListBinding"/>。
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            var initializers = VisitElementInitializerList(binding.Initializers);
            return binding.Update(binding.Member, initializers);
        }

        /// <summary>
        /// 访问 <see cref="NewExpression"/>。
        /// </summary>
        /// <param name="newExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitNew(NewExpression newExp)
        {
            var args = VisitMemberAndExpressionList(newExp.Arguments, newExp.Members);
            return newExp.Update(newExp.Constructor, args, newExp.Members);
        }

        /// <summary>
        /// 访问 <see cref="MemberInitExpression"/>。
        /// </summary>
        /// <param name="memberInitExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression memberInitExp)
        {
            var n = (NewExpression)VisitNew(memberInitExp.NewExpression);
            var bindings = VisitBindingList(memberInitExp.Bindings);
            return memberInitExp.Update(n, bindings);
        }

        /// <summary>
        /// 访问 <see cref="ListInitExpression"/>。
        /// </summary>
        /// <param name="listInitExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitListInit(ListInitExpression listInitExp)
        {
            var n = (NewExpression)VisitNew(listInitExp.NewExpression);
            var initializers = VisitElementInitializerList(listInitExp.Initializers);
            return listInitExp.Update(n, initializers);
        }

        /// <summary>
        /// 访问 <see cref="NewArrayExpression"/>。
        /// </summary>
        /// <param name="newArrayExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitNewArray(NewArrayExpression newArrayExp)
        {
            var exprs = VisitMemberAndExpressionList(newArrayExp.Expressions);
            return newArrayExp.Update(newArrayExp.Type, exprs);
        }

        /// <summary>
        /// 访问 <see cref="InvocationExpression"/>。
        /// </summary>
        /// <param name="invocationExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitInvocation(InvocationExpression invocationExp)
        {
            var args = VisitMemberAndExpressionList(invocationExp.Arguments);
            var expr = Visit(invocationExp.Expression);
            return invocationExp.Update(expr, args);
        }

        /// <summary>
        /// 访问 <see cref="TypeBinaryExpression"/>。
        /// </summary>
        /// <param name="typeBinExp">要访问的表达式。</param>
        /// <returns></returns>
        protected virtual Expression VisitTypeBinary(TypeBinaryExpression typeBinExp)
        {
            var exp = Visit(typeBinExp.Expression);
            return typeBinExp.Update(exp, typeBinExp.TypeOperand);
        }
#else
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
#endif

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
