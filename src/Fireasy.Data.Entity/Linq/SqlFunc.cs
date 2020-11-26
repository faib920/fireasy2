// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// SQL 函数。
    /// </summary>
    public static class SqlFunc
    {
        /// <summary>
        /// 取记录数。
        /// </summary>
        /// <returns></returns>
        [MethodCallBind(typeof(CountFunctionBinder))]
        public static int Count()
        {
            return 0;
        }

        /// <summary>
        /// 取字段的总和。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodCallBind(typeof(SumFunctionBinder))]
        public static T Sum<T>(T value)
        {
            return value;
        }

        /// <summary>
        /// 取字段的平均值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodCallBind(typeof(AverageFunctionBinder))]
        public static T Average<T>(T value)
        {
            return value;
        }

        /// <summary>
        /// 取字段的最大值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodCallBind(typeof(MaxFunctionBinder))]
        public static T Max<T>(T value)
        {
            return value;
        }

        /// <summary>
        /// 取字段的最小值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodCallBind(typeof(MinFunctionBinder))]
        public static T Min<T>(T value)
        {
            return value;
        }

        /// <summary>
        /// 判断字段是否为 NULL。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodCallBind(typeof(IsNullFunctionBinder))]
        public static bool IsNull(object value)
        {
            return true;
        }

        private abstract class AggregateFunctionBinder : IMethodCallBinder
        {
            private readonly AggregateType aggType;

            public AggregateFunctionBinder(AggregateType aggType)
            {
                this.aggType = aggType;
            }

            public Expression Bind(MethodCallBindContext context)
            {
                var argExp = context.Expression.Arguments.Count == 0 ? null : context.Visitor.Visit(context.Expression.Arguments[0]);
                return new AggregateExpression(context.Expression.Type, aggType, argExp, false);
            }
        }

        private class CountFunctionBinder : AggregateFunctionBinder
        {
            public CountFunctionBinder()
                : base(AggregateType.Count)
            {
            }
        }

        private class SumFunctionBinder : AggregateFunctionBinder
        {
            public SumFunctionBinder()
                : base(AggregateType.Sum)
            {
            }
        }

        private class AverageFunctionBinder : AggregateFunctionBinder
        {
            public AverageFunctionBinder()
                : base(AggregateType.Average)
            {
            }
        }

        private class MaxFunctionBinder : AggregateFunctionBinder
        {
            public MaxFunctionBinder()
                : base(AggregateType.Max)
            {
            }
        }

        private class MinFunctionBinder : AggregateFunctionBinder
        {
            public MinFunctionBinder()
                : base(AggregateType.Min)
            {
            }
        }

        private class IsNullFunctionBinder : IMethodCallBinder
        {
            public Expression Bind(MethodCallBindContext context)
            {
                var expression = context.Visitor.Visit(context.Expression.Arguments[0]);
                return new IsNullExpression(expression);
            }
        }
    }
}
