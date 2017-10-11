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
    public class PartialEvaluator
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
            canBeEvaluatedLocally = canBeEvaluatedLocally ?? CanBeEvaluatedLocally;
            return InternalPartialEvaluator.Eval(expression, canBeEvaluatedLocally);
        }

        /// <summary>
        /// 判断表达式中的常量是否可以被计算。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            var mc = expression as MethodCallExpression;
            if (mc != null &&
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

            private class SubtreeEvaluator : Common.Linq.Expressions.ExpressionVisitor
            {
                HashSet<Expression> candidates;

                private SubtreeEvaluator(HashSet<Expression> candidates)
                {
                    this.candidates = candidates;
                }

                internal static Expression Eval(HashSet<Expression> candidates, Expression exp)
                {
                    return new SubtreeEvaluator(candidates).Visit(exp);
                }

                public override Expression Visit(Expression exp)
                {
                    if (exp == null)
                    {
                        return null;
                    }

                    if (candidates.Contains(exp))
                    {
                        return Evaluate(exp);
                    }

                    return base.Visit(exp);
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
                            e = ce = Expression.Constant(ce.Value, type);
                        }

                        return e;
                    }

                    var me = e as MemberExpression;
                    if (me != null)
                    {
                        // member accesses off of constant's are common, and yet since these partial evals
                        // are never re-used, using reflection to access the member is faster than compiling  
                        // and invoking a lambda
                        var ce = me.Expression as ConstantExpression;
                        if (ce != null)
                        {
                            return Expression.Constant(me.Member.GetMemberValue(ce.Value), type);
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

            class Nominator : Common.Linq.Expressions.ExpressionVisitor
            {
                private readonly Func<Expression, bool> fnCanBeEvaluated;
                private readonly HashSet<Expression> candidates;
                private bool cannotBeEvaluated;

                private Nominator(Func<Expression, bool> fnCanBeEvaluated)
                {
                    candidates = new HashSet<Expression>();
                    this.fnCanBeEvaluated = fnCanBeEvaluated;
                }

                internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression)
                {
                    var nominator = new Nominator(fnCanBeEvaluated);
                    nominator.Visit(expression);
                    return nominator.candidates;
                }

                public override Expression Visit(Expression expression)
                {
                    if (expression != null)
                    {
                        var saveCannotBeEvaluated = cannotBeEvaluated;
                        cannotBeEvaluated = false;
                        base.Visit(expression);

                        if (!cannotBeEvaluated)
                        {
                            if (fnCanBeEvaluated(expression))
                            {
                                candidates.Add(expression);
                            }
                            else
                            {
                                cannotBeEvaluated = true;
                            }
                        }

                        cannotBeEvaluated |= saveCannotBeEvaluated;
                    }

                    return expression;
                }
            }
        }
    }
}
