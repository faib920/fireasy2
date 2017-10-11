// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Batcher;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.QueryBuilder;
using Fireasy.Data.Entity.Validation;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 提供实体的数据持久化支持。
    /// </summary>
    /// <typeparam name="TEntity">实体的类型。</typeparam>
    public class EntityPersister<TEntity> :
        IEntityPersistentEnvironment,
        IQueryProviderAware,
        IDisposable where TEntity : class, IEntity
    {
        private EntityPersistentEnvironment environment;
        private readonly IEntityQueryProvider entityQueryProvider;
        private readonly EntityMetadata metadata;
        private bool isDisposed;
        private Type entityType;
        internal InternalContext context;
        private IQueryProvider queryable;

        /// <summary>
        /// 使用实例名初始化 <see cref="T:Lord.Data.Entity.EntityPersister`1"/> 类的新实例。
        /// </summary>
        /// <param name="instanceName">实例名。</param>
        public EntityPersister(string instanceName = null)
        {
            metadata = EntityMetadataUnity.GetEntityMetadata(GetEntityType());
            context = new InternalContext(instanceName);
            Database = context.Database;
            entityQueryProvider = new EntityQueryProvider(context);
            queryable = new QueryProvider(entityQueryProvider);
            entityQueryProvider.InitializeInstanceName(instanceName);
            environment = context.Environment;
        }

        /// <summary>
        /// 使用 <see cref="IDatabase"/> 对象初始化 <see cref="T:Lord.Data.Entity.EntityPersister`1"/> 类的新实例。
        /// </summary>
        /// <param name="database">一个 <see cref="IDatabase"/> 对象。</param>
        public EntityPersister(IDatabase database)
        {
            Database = database;
            context = new InternalContext(database);
            entityQueryProvider = new EntityQueryProvider(context);
            queryable = new QueryProvider(entityQueryProvider);
            environment = context.Environment;
        }

        public EntityPersister(InternalContext context)
        {
            this.context = context;
            Database = context.Database;
            entityQueryProvider = new EntityQueryProvider(context);
            queryable = new QueryProvider(entityQueryProvider);
            environment = context.Environment;
        }

        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 对象。
        /// </summary>
        public IQueryProvider Provider
        {
            get { return queryable; }
        }

        /// <summary>
        /// 获取当前所使用的 <see cref="IDatabase"/> 对象。
        /// </summary>
        protected IDatabase Database { get; private set; }

        /// <summary>
        /// 获取或设置持久化环境。
        /// </summary>
        public virtual EntityPersistentEnvironment Environment
        {
            get
            {
                return environment;
            }
            set
            {
                environment = value;
                entityQueryProvider.InitializeEnvironment(value);
            }
        }

        /// <summary>
        /// 获取实体类型。
        /// </summary>
        /// <returns>持久化所操作的实体类型。</returns>
        public Type GetEntityType()
        {
            return entityType ?? (entityType = typeof(TEntity));
        }

        /// <summary>
        /// 将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> 参数为 null。</exception>
        public virtual void Create(TEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            ValidationUnity.Validate(entity);

            new QuerySet<TEntity>(Provider, null).CreateEntity(entity);
        }

        /// <summary>
        /// 批量将一组实体对象创建到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次写入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public virtual void BatchCreate(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            Guard.ArgumentNull(entities, "entities");

            var batcher = Database.Provider.GetService<IBatcherProvider>();
            if (batcher == null)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.NotSupportBatcher), null);
            }

            var syntax = Database.Provider.GetService<ISyntaxProvider>();
            var rootType = GetEntityType().GetRootType();
            var tableName = string.Empty;

            if (Environment != null)
            {
                tableName = DbUtility.FormatByQuote(syntax, Environment.GetVariableTableName(rootType));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(rootType);
                tableName = DbUtility.FormatByQuote(syntax, metadata.TableName);
            }

            batcher.Insert(Database, entities, tableName, batchSize, completePercentage);
        }

        /// <summary>
        /// 将实体对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        public virtual void Save(TEntity entity)
        {
            switch (entity.EntityState)
            {
                case EntityState.Modified:
                    Update(entity);
                    break;
                case EntityState.Attached:
                    Create(entity);
                    break;
                case EntityState.Detached:
                    Remove(entity);
                    break;
            }
        }

        /// <summary>
        /// 将一组实体对象的更改保存到库。不会更新实体的其他引用属性。
        /// </summary>
        /// <param name="entities">要保存的实体序列。</param>
        public virtual void Save(IEnumerable<TEntity> entities)
        {
            var query = new QuerySet<TEntity>(Provider, null);
#if NET35
            query.BatchOperate(entities.Cast<IEntity>().Where(s => s.EntityState == EntityState.Attached), query.CreateInsertExpression());
            query.BatchOperate(entities.Cast<IEntity>().Where(s => s.EntityState == EntityState.Modified), query.CreateUpdateExpression());
            query.BatchOperate(entities.Cast<IEntity>().Where(s => s.EntityState == EntityState.Detached), query.CreateDeleteExpression(true));
#else
            query.BatchOperate(entities.Where(s => s.EntityState == EntityState.Attached), query.CreateInsertExpression());
            query.BatchOperate(entities.Where(s => s.EntityState == EntityState.Modified), query.CreateUpdateExpression());
            query.BatchOperate(entities.Where(s => s.EntityState == EntityState.Detached), query.CreateDeleteExpression(true));
#endif
        }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">保存的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> 参数为 null。</exception>
        /// <returns>影响的实体数。</returns>
        public virtual int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            Guard.ArgumentNull(entity, "entity");

            return new QuerySet<TEntity>(Provider).UpdateWhere(entity, predicate);
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="accumulator">一个累加器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Update(Expression<Func<TEntity, TEntity>> accumulator, Expression<Func<TEntity, bool>> predicate)
        {
            return new QuerySet<TEntity>(Provider).UpdateWhere(accumulator, predicate);
        }

        /// <summary>
        /// 更新实体对象的修改。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> 参数为 null。</exception>
        public virtual void Update(TEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            ValidationUnity.Validate(entity);

            new QuerySet<TEntity>(Provider).UpdateEntity(entity);
        }

        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对数据假删除的支持。</param>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> 参数为 null。</exception>
        public virtual void Remove(TEntity entity, bool fake = true)
        {
            Guard.ArgumentNull(entity, "entity");

            new QuerySet<TEntity>(Provider).RemoveEntity(entity, fake);
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">主键的值。数组的长度必须与实体所定义的主键相匹配。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对对象假删除的支持。</param>
        /// <exception cref="ArgumentNullException"><paramref name="primaryValues"/> 参数为 null。</exception>
        public virtual void Remove(object[] primaryValues, bool fake = true)
        {
            Guard.ArgumentNull(primaryValues, "primaryValues");

            new QuerySet<TEntity>(Provider).RemoveByPrimary(primaryValues, fake);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="fake">如果具有 IsDeletedKey 的属性，则提供对对象假删除的支持。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Remove(Expression<Func<TEntity, bool>> predicate = null, bool fake = true)
        {
            return new QuerySet<TEntity>(Provider).RemoveWhere(predicate, fake);
        }

        /// <summary>
        /// 返回满足条件的一组实体对象。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>一个实体查询集。</returns>
        public virtual QuerySet<TEntity> Query(Expression<Func<TEntity, bool>> predicate = null)
        {
            return QueryHelper.CreateQuery<TEntity>(Provider, predicate);
        }

        /// <summary>
        /// 返回满足条件的一组对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>一个实体查询集。</returns>
        public virtual QuerySet<T> Query<T>(Expression<Func<T, bool>> predicate = null)
        {
            return QueryHelper.CreateQuery<T>(Provider, predicate);
        }

        /// <summary>
        /// 根据自定义的SQL语句查询返回一组对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns><typeparamref name="T"/> 类型的对象枚举器。</returns>
        public virtual IEnumerable<T> Query<T>(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null) where T : new()
        {
            return Database.InternalExecuteEnumerable<T>(queryCommand, segment, parameters);
        }

        /// <summary>
        /// 根据自定义的SQL语句查询返回一组动态对象。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>一个动态类型的对象枚举器。</returns>
        public virtual IEnumerable Query(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            return Database.ExecuteEnumerable(queryCommand, segment, parameters);
        }

        /// <summary>
        /// 返回满足条件的一组实体对象。
        /// </summary>
        /// <param name="condition">一般的条件语句。</param>
        /// <param name="orderBy">排序语句。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns>当前类型的实体枚举器。</returns>
        public virtual IEnumerable<TEntity> Query(string condition, string orderBy, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            var syntax = Database.Provider.GetService<ISyntaxProvider>();
            var query = new EntityQueryBuilder(syntax, Environment, GetEntityType(), parameters).Select().All().From().Where(condition).OrderBy(orderBy);
            return Database.InternalExecuteEnumerable<TEntity>(query.ToSqlCommand(), segment, parameters);
        }

        /// <summary>
        /// 使用主键值查询返回一个实体。
        /// </summary>
        /// <param name="primaryValues">主键的值。数组的长度必须与实体所定义的主键相匹配。</param>
        /// <exception cref="ArgumentNullException"><paramref name="primaryValues"/> 参数为 null。</exception>
        /// <returns>一个实体对象。</returns>
        public virtual TEntity First(params object[] primaryValues)
        {
            Guard.ArgumentNull(primaryValues, "primaryValues");

            var pkProperties = PropertyUnity.GetPrimaryProperties(GetEntityType()).ToList();
            var realLength = primaryValues == null ? 0 : primaryValues.Length;
            if (realLength != pkProperties.Count)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.DisaccordArgument, pkProperties.Count, realLength), null);
            }

            var parExp = Expression.Parameter(GetEntityType(), "s");
            Expression binExp = null;
            var i = 0;
            foreach (var property in pkProperties)
            {
                var proExp = Expression.Property(parExp, property.Info.ReflectionInfo);
                var rightExp = proExp.Equal(Expression.Constant(primaryValues[i].ToType(property.Type)));
                binExp = binExp == null ? rightExp : Expression.And(binExp, rightExp);
                i++;
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(binExp, parExp);
            return Query(lambda).FirstOrDefault();
        }

        /// <summary>
        /// 指定要包括在查询结果中的关联对象。
        /// </summary>
        /// <param name="fnMember">要包含的属性的表达式。</param>
        /// <returns></returns>
        public EntityPersister<TEntity> Include(Expression<Func<TEntity, object>> fnMember)
        {
            context.IncludeWith(fnMember);
            return this;
        }

        /// <summary>
        /// 对关联对象的查询采用指定的谓语。
        /// </summary>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        public EntityPersister<TEntity> Associate(Expression<Func<TEntity, IEnumerable>> memberQuery)
        {
            context.AssociateWith(memberQuery);
            return this;
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (Database != null)
            {
                Database.Dispose();
            }

            isDisposed = true;
        }
    }
}