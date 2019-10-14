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
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 核心的组件，用于管理上下文中的各种组件。
    /// </summary>
    public sealed class DefaultContextService :
        ContextServiceBase,
        IQueryPolicy
    {
        private HashSet<MemberInfo> included = new HashSet<MemberInfo>();
        private Dictionary<MemberInfo, List<LambdaExpression>> operations = new Dictionary<MemberInfo, List<LambdaExpression>>();

        /// <summary>
        /// 初始化 <see cref="DefaultContextService"/> 类的新实例。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="databaseFactory">一个用于创建 <see cref="IDatabase"/> 的工厂函数。</param>
        public DefaultContextService(EntityContextInitializeContext context, Func<IProvider, ConnectionString, IDatabase> databaseFactory)
            : base (context)
        {
            var options = context.Options;

            Func<IDatabase> factory = null;
            if (DatabaseScope.Current != null)
            {
                factory = () => DatabaseScope.Current.Database;
            }
            else if (databaseFactory != null)
            {
                factory = () => databaseFactory(context.Provider, context.ConnectionString);
            }
            else if (context.Provider != null && context.ConnectionString != null)
            {
                factory = () => new Database(context.ConnectionString, context.Provider);
            }
            else if (options != null)
            {
                factory = () => DatabaseFactory.CreateDatabase(options.ConfigName);
            }

            if (factory != null)
            {
                Database = EntityDatabaseFactory.CreateDatabase(InstanceName, factory);
                Provider = Database.Provider;
            }
        }

        bool IQueryPolicy.IsIncluded(MemberInfo member)
        {
            return included.Contains(member);
        }

        Expression IQueryPolicy.ApplyPolicy(Expression expression, MemberInfo member)
        {
            if (operations.TryGetValue(member, out List<LambdaExpression> ops))
            {
                var syntax = Database.Provider.GetService<ISyntaxProvider>();
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
                    projection = new ProjectionExpression(projection.Select, projection.Projector, fnAgg, projection.IsAsync);
                }

                return projection;
            }

            return expression;
        }

        /// <summary>
        /// 开始事务。
        /// </summary>
        /// <param name="level"></param>
        public override void BeginTransaction(IsolationLevel level)
        {
            Database?.BeginTransaction(level);
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public override void CommitTransaction()
        {
            Database?.CommitTransaction();
        }

        /// <summary>
        /// 回滚事务。
        /// </summary>
        public override void RollbackTransaction()
        {
            Database?.RollbackTransaction();
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

        public override void Dispose()
        {
            Database?.Dispose();
        }

        private class RootMemberFinder : Fireasy.Common.Linq.Expressions.ExpressionVisitor
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
                    this.Visit(m.Object);
                }
                else if (m.Arguments.Count > 0)
                {
                    this.Visit(m.Arguments[0]);
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

        private class MemberFinder : Fireasy.Common.Linq.Expressions.ExpressionVisitor
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
