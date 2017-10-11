// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 核心的组件，用于管理上下文中的各种组件。
    /// </summary>
    public sealed class InternalContext :
        IQueryPolicy,
        IDisposable,
        IEntityPersistentInstanceContainer,
        IEntityPersistentEnvironment
    {
        private HashSet<MemberInfo> included = new HashSet<MemberInfo>();
        private Dictionary<MemberInfo, List<LambdaExpression>> operations = new Dictionary<MemberInfo, List<LambdaExpression>>();
        private Dictionary<Type, IRepository> holders = new Dictionary<Type, IRepository>();

        public InternalContext(string name)
        {
            Database = EntityDatabaseFactory.CreateDatabase(name);
            InstanceName = name;
        }

        public InternalContext(IDatabase database)
        {
            Database = database;
        }

        public string InstanceName { get; set; }

        public EntityPersistentEnvironment Environment { get; set; }

        public IDatabase Database { get; internal set; }

        public Action<Type> OnRespositoryCreated { get; set; }

        bool IQueryPolicy.IsIncluded(MemberInfo member)
        {
            return included.Contains(member);
        }

        Expression IQueryPolicy.ApplyPolicy(Expression expression, MemberInfo member)
        {
            List<LambdaExpression> ops;
            if (operations.TryGetValue(member, out ops))
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
                    projection = new ProjectionExpression(projection.Select, projection.Projector, fnAgg);
                }

                return projection;
            }

            return expression;
        }

        public IRepository GetDbSet(Type entitytype)
        {
            IRepository set;
            if (!this.holders.TryGetValue(entitytype, out set))
            {
                set = CreateDbSet(entitytype);
                lock (this.holders)
                {
                    holders[entitytype] = set;
                }
            }

            return set;
        }

        public IRepositoryProvider<TEntity> CreateRepositoryProvider<TEntity>() where TEntity : IEntity
        {
            var factory = Database.Provider.GetService<IContextProvider>() ?? new DefaultContextProvider();
            RespositoryCreator.TryCreate(typeof(TEntity), this);
            return factory.Create<TEntity>(this);
        }

        public IRepositoryProvider CreateRepositoryProvider(Type entityType)
        {
            var factory = Database.Provider.GetService<IContextProvider>() ?? new DefaultContextProvider();
            RespositoryCreator.TryCreate(entityType, this);
            return factory.Create(entityType, this);
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
            if (fnApply == null)
                throw new ArgumentNullException("fnApply");
            if (fnApply.Parameters.Count != 1)
                throw new ArgumentException("Apply function has wrong number of arguments.");
            AddOperation(fnApply.Parameters[0].Type.GetEnumerableElementType(), fnApply);
        }

        public void Apply(Type entityType, LambdaExpression fnApply)
        {
            if (fnApply == null)
                throw new ArgumentNullException("fnApply");
            if (fnApply.Parameters.Count != 1)
                throw new ArgumentException("Apply function has wrong number of arguments.");
            AddOperation(entityType, fnApply);
        }

        private void AddOperation(MemberInfo member, LambdaExpression operation)
        {
            List<LambdaExpression> memberOps;
            if (!operations.TryGetValue(member, out memberOps))
            {
                memberOps = new List<LambdaExpression>();
                operations.Add(member, memberOps);
            }

            memberOps.Add(operation);
        }

        public void Dispose()
        {
            Database.Dispose();
        }

        private IRepository CreateDbSet(Type entityType)
        {
            return typeof(EntityRepository<>).MakeGenericType(new Type[] { entityType }).New(this) as IRepository;
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
