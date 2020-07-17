// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Common.Linq.Expressions
{
    /// <summary>
    /// 用于计算表达式中的常量表达式。
    /// </summary>
    public static class PartialEvaluator
    {
        /// <summary>
        /// 计算表达式中的常量表达式。
        /// </summary>
        /// <param name="expression">要计算的表达式。</param>
        /// <param name="canBeEvaluatedLocally">一个函数，指示哪些表达式可以被计算。</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>如有以下的表达式：</para>
        /// <para>var v = new { People = new { Age = 34 } };</para>
        /// <para>var exp = s =&gt; s.Age == v.People.Age || s.Age == new { Age = 56 }.Age;</para>
        /// <para>使用该方法计算 exp 后为：</para>
        /// <para>s =&gt; s.Age == 34 || s.Age == 56;</para>
        /// </remarks>
        public static Expression Eval(Expression expression, Func<Expression, bool> canBeEvaluatedLocally = null)
        {
            canBeEvaluatedLocally ??= CanBeEvaluatedLocally;
            return InternalPartialEvaluator.Eval(expression, canBeEvaluatedLocally);
        }

        /// <summary>
        /// 判断表达式中的常量是否可以被计算。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            if (expression is MethodCallExpression mc &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable)))
            {
                return false;
            }

            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
            {
                return true;
            }

            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        class InternalPartialEvaluator
        {
            public static Expression Eval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
            {
                return SubtreeEvaluator.Eval(Nominator.Nominate(fnCanBeEvaluated, expression), expression);
            }

            public static Expression Eval(Expression expression)
            {
                return Eval(expression, CanBeEvaluatedLocally);
            }

            private static bool CanBeEvaluatedLocally(Expression expression)
            {
                return expression.NodeType != ExpressionType.Parameter;
            }

            private class SubtreeEvaluator : ExpressionVisitor
            {
                private readonly HashSet<Expression> _candidates;

                private SubtreeEvaluator(HashSet<Expression> candidates)
                {
                    _candidates = candidates;
                }

                internal static Expression Eval(HashSet<Expression> candidates, Expression exp)
                {
                    return new SubtreeEvaluator(candidates).Visit(exp);
                }

                public override Expression Visit(Expression expression)
                {
                    if (expression == null)
                    {
                        return null;
                    }

                    var query = QueryableHelper.GetQuerableMember(expression);
                    if (query != null)
                    {
                        return Visit(query.Expression);
                    }
                    else if (_candidates.Contains(expression))
                    {
                        return Evaluate(expression);
                    }

                    return base.Visit(expression);
                }

                private Expression Evaluate(Expression e)
                {
                    Type type = e.Type;

                    // check for nullable converts & strip them
                    if (e.NodeType == ExpressionType.Convert)
                    {
                        var u = (UnaryExpression)e;
                        if (u.Operand.Type.GetNonNullableType() == type.GetNonNullableType())
                        {
                            e = ((UnaryExpression)e).Operand;
                        }
                    }

                    // if we now just have a constant, return it
                    if (e.NodeType == ExpressionType.Constant)
                    {
                        var ce = (ConstantExpression)e;

                        // if we've lost our nullable typeness add it back
                        if (e.Type != type && e.Type.GetNonNullableType() == type.GetNonNullableType())
                        {
                            e = Expression.Constant(ce.Value, type);
                        }

                        return e;
                    }

                    if (e is MemberExpression me)
                    {
                        // member accesses off of constant's are common, and yet since these partial evals
                        // are never re-used, using reflection to access the member is faster than compiling  
                        // and invoking a lambda
                        if (me.Expression is ConstantExpression ce)
                        {
                            var value = me.Member.GetMemberValue(ce.Value);
                            if (value is IQueryable queryable)
                            {
                                _ = Visit(queryable.Expression);
                            }
                            else
                            {
                                return Expression.Constant(value, type);
                            }
                        }
                    }

                    if (type.IsValueType)
                    {
                        e = Expression.Convert(e, typeof(object));
                    }

                    var lambda = Expression.Lambda<Func<object>>(e);
                    var fn = lambda.Compile();
                    return Expression.Constant(fn(), type);
                }
            }

            class Nominator : ExpressionVisitor
            {
                private readonly Func<Expression, bool> _fnCanBeEvaluated;
                private readonly HashSet<Expression> _candidates;
                private bool _cannotBeEvaluated;

                private Nominator(Func<Expression, bool> fnCanBeEvaluated)
                {
                    _candidates = new HashSet<Expression>();
                    _fnCanBeEvaluated = fnCanBeEvaluated;
                }

                internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression)
                {
                    var nominator = new Nominator(fnCanBeEvaluated);
                    nominator.Visit(expression);
                    return nominator._candidates;
                }

                public override Expression Visit(Expression expression)
                {
                    if (expression != null)
                    {
                        var saveCannotBeEvaluated = _cannotBeEvaluated;
                        _cannotBeEvaluated = false;

                        var query = QueryableHelper.GetQuerableMember(expression);
                        if (query != null)
                        {
                            Visit(query.Expression);
                        }
                        else
                        {
                            base.Visit(expression);
                        }

                        if (!_cannotBeEvaluated)
                        {
                            if (_fnCanBeEvaluated(expression))
                            {
                                _candidates.Add(expression);
                            }
                            else
                            {
                                _cannotBeEvaluated = true;
                            }
                        }

                        _cannotBeEvaluated |= saveCannotBeEvaluated;
                    }

                    return expression;
                }
            }

            class QueryableHelper
            {
                internal static IQueryable GetQuerableMember(Expression expression)
                {
                    if (expression is MemberExpression member && typeof(IQueryable).IsAssignableFrom(member.Type) &&
                        member.Expression is ConstantExpression constant)
                    {
                        var value = member.Member.GetMemberValue(constant.Value);
                        return value as IQueryable;
                    }

                    return null;
                }
            }
        }
    }
}
