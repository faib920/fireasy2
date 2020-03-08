// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq
{
    public class DefaultQueryPolicy : IQueryPolicy
    {
        private readonly HashSet<MemberInfo> included = new HashSet<MemberInfo>();
        private readonly Dictionary<MemberInfo, List<LambdaExpression>> operations = new Dictionary<MemberInfo, List<LambdaExpression>>();
        private IProvider provider;

        public DefaultQueryPolicy(IProvider provider)
        {
            this.provider = provider;
        }

        public bool IsIncluded(MemberInfo member)
        {
            return included.Contains(member);
        }

        public Expression ApplyPolicy(Expression expression, MemberInfo member)
        {
            if (operations.TryGetValue(member, out List<LambdaExpression> ops))
            {
                var syntax = provider.GetService<ISyntaxProvider>();
                var result = expression;
                foreach (var fnOp in ops)
                {
                    var pop = PartialEvaluator.Eval(fnOp);
                    result = QueryBinder.Bind(Expression.Invoke(pop, result), syntax);
                }

                var projection = (ProjectionExpression)result;
                if (projection.Type != expression.Type)
                {
                    var fnAgg = QueryUtility.GetAggregator(expression.Type, projection.Type);
                    projection = new ProjectionExpression(projection.Select, projection.Projector, fnAgg, projection.IsAsync, projection.IsNoTracking);
                }

                return projection;
            }

            return expression;
        }

        public void IncludeWith<TEntity>(Expression<Func<TEntity, object>> fnMember) where TEntity : IEntity
        {
            foreach (var member in MemberFinder.Find(fnMember))
            {
                included.Add(member);
            }
        }

        public void AssociateWith<TEntity>(Expression<Func<TEntity, IEnumerable>> memberQuery) where TEntity : IEntity
        {
            var rootMember = RootMemberFinder.Find(memberQuery, memberQuery.Parameters[0]);
            if (rootMember != memberQuery.Body)
            {
                var memberParam = Expression.Parameter(rootMember.Type, "root");
                var newBody = DbExpressionReplacer.Replace(memberQuery.Body, rootMember, memberParam);
                AddOperation(rootMember.Member, Expression.Lambda(newBody, memberParam));
            }
        }

        public void Apply<TEntity>(Expression<Func<IEnumerable<TEntity>, IEnumerable<TEntity>>> fnApply) where TEntity : IEntity
        {
            Guard.ArgumentNull(fnApply, nameof(fnApply));
            Guard.Argument(fnApply.Parameters.Count == 1, nameof(fnApply));

            AddOperation(fnApply.Parameters[0].Type.GetEnumerableElementType(), fnApply);
        }

        public void Apply(Type entityType, LambdaExpression fnApply)
        {
            Guard.ArgumentNull(fnApply, nameof(fnApply));
            Guard.Argument(fnApply.Parameters.Count == 1, nameof(fnApply));

            AddOperation(entityType, fnApply);
        }

        private void AddOperation(MemberInfo member, LambdaExpression operation)
        {
            if (!operations.TryGetValue(member, out List<LambdaExpression> memberOps))
            {
                memberOps = new List<LambdaExpression>();
                operations.Add(member, memberOps);
            }

            memberOps.Add(operation);
        }

        private class RootMemberFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            MemberExpression found;
            ParameterExpression parameter;

            public static MemberExpression Find(Expression expression, ParameterExpression parameter)
            {
                var mf = new RootMemberFinder { parameter = parameter };
                mf.Visit(expression);
                return mf.found;
            }

            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (m.Object != null)
                {
                    Visit(m.Object);
                }
                else if (m.Arguments.Count > 0)
                {
                    Visit(m.Arguments[0]);
                }
                return m;
            }

            protected override Expression VisitMember(MemberExpression m)
            {
                if (m.Expression == this.parameter)
                {
                    found = m;
                    return m;
                }
                else
                {
                    return base.VisitMember(m);
                }
            }

        }

        private class MemberFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private List<MemberInfo> members = new List<MemberInfo>();

            public static IEnumerable<MemberInfo> Find(Expression expression)
            {
                var mf = new MemberFinder();
                mf.Visit(expression);
                return mf.members;
            }

            protected override Expression VisitMember(MemberExpression memberExp)
            {
                var ex = Visit(memberExp.Expression);

                var member = memberExp.Member;
                var p = PropertyUnity.GetProperty(member.DeclaringType, member.Name);
                if (p != null && p is IPropertyLazy)
                {
                    members.Add(member);
                }

                return memberExp;
            }
        }
    }
}
