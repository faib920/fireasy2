// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 表达式的扩展方法。
    /// </summary>
    public static class ExpressionExtension
    {
        /// <summary>
        /// 创建一个表示两个表达式相等的表达式。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression Equal(this Expression left, Expression right)
        {
            ConvertExpressions(ref left, ref right);
            return Expression.Equal(left, right);
        }

        /// <summary>
        /// 创建一个表示两个表达式不相等的表达式。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression NotEqual(this Expression left, Expression right)
        {
            ConvertExpressions(ref left, ref right);
            return Expression.NotEqual(left, right);
        }

        /// <summary>
        /// 创建一个表示大于的表达式。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression GreaterThan(this Expression left, Expression right)
        {
            ConvertExpressions(ref left, ref right);
            return Expression.GreaterThan(left, right);
        }

        /// <summary>
        /// 创建一个表示大于等于的表达式。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression GreaterThanOrEqual(this Expression left, Expression right)
        {
            ConvertExpressions(ref left, ref right);
            return Expression.GreaterThanOrEqual(left, right);
        }

        /// <summary>
        /// 创建一个表示小于的表达式。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression LessThan(this Expression left, Expression right)
        {
            ConvertExpressions(ref left, ref right);
            return Expression.LessThan(left, right);
        }

        /// <summary>
        /// 创建一个表示小于等于的表达式。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression LessThanOrEqual(this Expression left, Expression right)
        {
            ConvertExpressions(ref left, ref right);
            return Expression.LessThanOrEqual(left, right);
        }

        /// <summary>
        /// 创建一个按与运算的表达式。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression And(this Expression left, Expression right)
        {
            ConvertExpressions(ref left, ref right);
            return Expression.And(left, right);
        }

        /// <summary>
        /// 创建一个按或运算的表达式。
        /// </summary>
        /// <param name="expression1"></param>
        /// <param name="expression2"></param>
        /// <returns></returns>
        public static Expression Or(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.Or(expression1, expression2);
        }

        /// <summary>
        /// 转换两个表达式。如果其中一个是可空类型，则转换另一个为可空类型。
        /// </summary>
        /// <param name="expression1"></param>
        /// <param name="expression2"></param>
        private static void ConvertExpressions(ref Expression expression1, ref Expression expression2)
        {
            if (expression1.Type != expression2.Type)
            {
                var isNullable1 = expression1.Type.IsNullableType();
                var isNullable2 = expression2.Type.IsNullableType();
                if (isNullable1 || isNullable2)
                {
                    if (expression1.Type.GetNonNullableType() == expression2.Type.GetNonNullableType())
                    {
                        if (!isNullable1)
                        {
                            expression1 = Expression.Convert(expression1, expression2.Type);
                        }
                        else if (!isNullable2)
                        {
                            expression2 = Expression.Convert(expression2, expression1.Type);
                        }
                    }
                }
            }
        }

        public static Expression[] Split(this Expression expression, params ExpressionType[] binarySeparators)
        {
            var list = new List<Expression>();
            Split(expression, list, binarySeparators);
            return list.ToArray();
        }

        private static void Split(Expression expression, List<Expression> list, ExpressionType[] binarySeparators)
        {
            if (expression != null)
            {
                if (binarySeparators.Contains(expression.NodeType))
                {
                    if (expression is BinaryExpression bex)
                    {
                        Split(bex.Left, list, binarySeparators);
                        Split(bex.Right, list, binarySeparators);
                    }
                }
                else
                {
                    list.Add(expression);
                }
            }
        }

        public static Expression Join(this IEnumerable<Expression> list, ExpressionType binarySeparator)
        {
            if (list != null)
            {
                var array = list.ToArray();
                if (array.Length > 0)
                {
                    return array.Aggregate((x1, x2) => Expression.MakeBinary(binarySeparator, x1, x2));
                }
            }

            return null;
        }

        /// <summary>
        /// 使用 And 运算合并另一个表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> current, Expression<Func<T, bool>> other)
        {
            var newExp = Complex(current, other, (left, right) => Expression.And(left, right));
            if (newExp == null)
            {
                return null;
            }
            else if (newExp.NodeType == ExpressionType.Lambda)
            {
                return (Expression<Func<T, bool>>)newExp;
            }

            return Expression.Lambda<Func<T, bool>>(newExp, current.Parameters);
        }

        /// <summary>
        /// 使用 Or 运算合并另一个表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> current, Expression<Func<T, bool>> other)
        {
            var newExp = Complex(current, other, (left, right) => Expression.Or(left, right));
            if (newExp == null)
            {
                return null;
            }
            else if (newExp.NodeType == ExpressionType.Lambda)
            {
                return (Expression<Func<T, bool>>)newExp;
            }

            return Expression.Lambda<Func<T, bool>>(newExp, current.Parameters);
        }

        /// <summary>
        /// 使用 And 运算合并另一个表达式。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> And<T1, T2>(this Expression<Func<T1, T2, bool>> current, Expression<Func<T1, T2, bool>> other)
        {
            var newExp = Complex(current, other, (left, right) => Expression.And(left, right));
            if (newExp == null)
            {
                return null;
            }
            else if (newExp.NodeType == ExpressionType.Lambda)
            {
                return (Expression<Func<T1, T2, bool>>)newExp;
            }

            return Expression.Lambda<Func<T1, T2, bool>>(newExp, current.Parameters);
        }

        /// <summary>
        /// 使用 Or 运算合并另一个表达式。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> Or<T1, T2>(this Expression<Func<T1, T2, bool>> current, Expression<Func<T1, T2, bool>> other)
        {
            var newExp = Complex(current, other, (left, right) => Expression.Or(left, right));
            if (newExp == null)
            {
                return null;
            }
            else if (newExp.NodeType == ExpressionType.Lambda)
            {
                return (Expression<Func<T1, T2, bool>>)newExp;
            }

            return Expression.Lambda<Func<T1, T2, bool>>(newExp, current.Parameters);
        }

        /// <summary>
        /// 使用 And 运算合并另一个表达式。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> And<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> current, Expression<Func<T1, T2, T3, bool>> other)
        {
            var newExp = Complex(current, other, (left, right) => Expression.And(left, right));
            if (newExp == null)
            {
                return null;
            }
            else if (newExp.NodeType == ExpressionType.Lambda)
            {
                return (Expression<Func<T1, T2, T3, bool>>)newExp;
            }

            return Expression.Lambda<Func<T1, T2, T3, bool>>(newExp, current.Parameters);
        }

        /// <summary>
        /// 使用 Or 运算合并另一个表达式。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> Or<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> current, Expression<Func<T1, T2, T3, bool>> other)
        {
            var newExp = Complex(current, other, (left, right) => Expression.Or(left, right));
            if (newExp == null)
            {
                return null;
            }
            else if (newExp.NodeType == ExpressionType.Lambda)
            {
                return (Expression<Func<T1, T2, T3, bool>>)newExp;
            }

            return Expression.Lambda<Func<T1, T2, T3, bool>>(newExp, current.Parameters);
        }

        private static Expression Complex(this LambdaExpression current, LambdaExpression other, Func<Expression, Expression, Expression> func)
        {
            if (current == null && other == null)
            {
                return null;
            }

            if (other == null)
            {
                return current;
            }

            if (current == null)
            {
                return other;
            }

            if (current.Parameters.Count != other.Parameters.Count)
            {
                return null;
            }

            var pars = current.Parameters;

            var left = current.Body;
            var right = other.Body;
            right = ExpressionReplacer.Replace(right, pars);

            return func(left, right);
        }
    }
}
