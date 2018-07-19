// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// SQL 函数。
    /// </summary>
    public static class SqlFunc
    {
        [MethodCallBind(typeof(CountFunctionBinder))]
        public static int Count()
        {
            return 0;
        }

        [MethodCallBind(typeof(SumFunctionBinder))]
        public static T Sum<T>(T value)
        {
            return value;
        }

        [MethodCallBind(typeof(AverageFunctionBinder))]
        public static T Average<T>(T value)
        {
            return value;
        }

        [MethodCallBind(typeof(MaxFunctionBinder))]
        public static T Max<T>(T value)
        {
            return value;
        }

        [MethodCallBind(typeof(MinFunctionBinder))]
        public static T Min<T>(T value)
        {
            return value;
        }

        private abstract class AggregateFunctionBinder : IMethodCallBinder
        {
            private AggregateType aggType;

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
                : base (AggregateType.Sum)
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
    }
}
