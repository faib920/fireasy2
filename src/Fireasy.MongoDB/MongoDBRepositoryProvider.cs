// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Metadata;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.MongoDB
{
    /// <summary>
    /// MongoDB 的仓储服务实现，由程序集 MongoSharpDeiver 提供。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class MongoDBRepositoryProvider<TEntity> : IRepositoryProvider<TEntity> where TEntity : class, IEntity
    {
        private static readonly SafetyDictionary<Type, CustomBsonSerializer> _cache = new SafetyDictionary<Type, CustomBsonSerializer>();

        private readonly MongoDBContextService _contextService;
        private readonly MongoCollection<TEntity> _collection;
        private IRepository _repository;

        /// <summary>
        /// 初始化 <see cref="MongoDBRepositoryProvider{TEntity}"/> 类的新实例。
        /// </summary>
        /// <param name="contextService"></param>
        public MongoDBRepositoryProvider(MongoDBContextService contextService)
        {
            _contextService = contextService;
            var metadata = EntityMetadataUnity.GetEntityMetadata(typeof(TEntity));
            var collectionSettings = new MongoCollectionSettings { AssignIdOnInsert = false };
            _collection = contextService.Database.GetCollection<TEntity>(metadata.TableName, collectionSettings);

            _cache.GetOrAdd(typeof(TEntity), contextService.ContextType, (k, c) =>
                {
                    var serializer = new CustomBsonSerializer(c);
                    BsonSerializer.RegisterSerializer(serializer);
                    return serializer;
                });

            var provider = new MongoQueryProvider(_collection);
            QueryProvider = provider;
            Queryable = new MongoQueryable<TEntity>(provider);
        }

        IRepository IRepositoryProvider.CreateRepository(EntityContextOptions options)
        {
            return _repository ?? (_repository = new EntityRepository<TEntity>(_contextService, this, options));
        }

        /// <summary>
        /// 获取 <see cref="IQueryable"/> 实例。
        /// </summary>
        public IQueryable Queryable { get; private set; }

        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 实例。
        /// </summary>
        public IQueryProvider QueryProvider { get; private set; }

        /// <summary>
        /// 将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public long Insert(TEntity entity, IPropertyFilter propertyFilter)
        {
            var result = _collection.Insert(entity);
            return result.DocumentsAffected;
        }

        /// <summary>
        /// 将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<long> InsertAsync(TEntity entity, IPropertyFilter propertyFilter = null, CancellationToken cancellationToken = default)
        {
            return Insert(entity, propertyFilter);
        }

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity, IPropertyFilter propertyFilter = null)
        {
            var result = _collection.Save(entity);
            return (int)result.DocumentsAffected;
        }

        /// <summary>
        /// 异步的，更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, IPropertyFilter propertyFilter = null, CancellationToken cancellationToken = default)
        {
            return Update(entity, propertyFilter);
        }

        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">此参数无效。</param>
        /// <param name="completePercentage">此参数无效。</param>
        public void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            _collection.InsertBatch(entities);
        }

        /// <summary>
        /// 异步的，批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">此参数无效。</param>
        /// <param name="completePercentage">此参数无效。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            BatchInsert(entities, batchSize, completePercentage);
        }

        /// <summary>
        /// 将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(TEntity entity, bool logicalDelete = true)
        {
            var predicate = BuildPrimaryExpression((p, i) => entity.GetValue(p).GetValue());
            var query = Query<TEntity>.Where(predicate);
            var result = _collection.Remove(query);
            return (int)result.DocumentsAffected;
        }

        /// <summary>
        /// 异步的，将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            return Delete(entity, logicalDelete);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true)
        {
            var query = Query<TEntity>.Where(predicate);
            var result = _collection.Remove(query);
            return (int)result.DocumentsAffected;
        }

        /// <summary>
        /// 异步的，将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            return Delete(predicate, logicalDelete);
        }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate, IPropertyFilter propertyFilter = null)
        {
            var query = Query<TEntity>.Where(predicate);
            var update = GetUpdateBuilder(entity);
            var result = _collection.Update(query, update);
            return (int)result.DocumentsAffected;
        }

        /// <summary>
        /// 异步的，使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, IPropertyFilter propertyFilter = null, CancellationToken cancellationToken = default)
        {
            return Update(entity, predicate, propertyFilter);
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步的，使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <returns>影响的实体数。</returns>
        public int Batch(IEnumerable<TEntity> instances, Expression<Action<IRepository<TEntity>, TEntity>> fnOperation, BatchOperateOptions batchOpt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步的，对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Action<IRepository<TEntity>, TEntity>> fnOperation, BatchOperateOptions batchOpt, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns></returns>
        public int Delete(PropertyValue[] primaryValues, bool logicalDelete = true)
        {
            var predicate = BuildPrimaryExpression((p, i) => PropertyValue.IsEmpty(primaryValues[i]) ? null : primaryValues[i].GetValue());
            var query = Query<TEntity>.Where(predicate);
            var result = _collection.Remove(query);
            return (int)result.DocumentsAffected;
        }

        /// <summary>
        /// 异步的，根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            return Delete(primaryValues, logicalDelete);
        }

        /// <summary>
        /// 通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        public TEntity Get(PropertyValue[] primaryValues)
        {
            var predicate = BuildPrimaryExpression((p, i) => PropertyValue.IsEmpty(primaryValues[i]) ? null : primaryValues[i].GetValue());
            var query = Query<TEntity>.Where(predicate);
            return _collection.FindOne(query);
        }

        /// <summary>
        /// 异步的，通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(PropertyValue[] primaryValues, CancellationToken cancellationToken = default)
        {
            return Get(primaryValues);
        }

        /// <summary>
        /// 构造通过主键查询的表达式。
        /// </summary>
        /// <param name="getFunc">获取主键值的函数。</param>
        /// <returns></returns>
        private Expression<Func<TEntity, bool>> BuildPrimaryExpression(Func<IProperty, int, object> getFunc)
        {
            var parExp = Expression.Parameter(typeof(TEntity), "s");
            var pkProperties = PropertyUnity.GetPrimaryProperties(typeof(TEntity));
            var eqExps = new List<Expression>();
            var index = 0;
            foreach (var property in pkProperties)
            {
                var value = getFunc(property, index++);
                if (value == null)
                {
                    continue;
                }

                Expression valExp = Expression.Constant(value);
                if (value.GetType() != property.Type)
                {
                    valExp = Expression.Convert(valExp, property.Type);
                }

                var memExp = Expression.MakeMemberAccess(parExp, property.Info.ReflectionInfo);
                eqExps.Add(Expression.Equal(memExp, valExp));
            }

            var expression = eqExps.Aggregate(Expression.And);
            return Expression.Lambda<Func<TEntity, bool>>(expression, parExp);
        }

        private UpdateBuilder<TEntity> GetUpdateBuilder(IEntity entity)
        {
            var method = typeof(UpdateBuilder<TEntity>).GetMethod("Set");

            var builder = new UpdateBuilder<TEntity>();
            var properties = PropertyUnity.GetPersistentProperties(typeof(TEntity));
            foreach (var property in properties)
            {
                var value = entity.GetValue(property);
                if (PropertyValue.IsEmpty(value))
                {
                    continue;
                }

                var parExp = Expression.Parameter(typeof(TEntity), "s");
                var memExp = Expression.MakeMemberAccess(parExp, property.Info.ReflectionInfo);
                var lbdExp = Expression.Lambda(memExp, parExp);

                method.MakeGenericMethod(property.Type).Invoke(builder, new object[] { lbdExp, value.GetValue() });
            }

            return builder;
        }

        /// <summary>
        /// 自定义的序列化器。
        /// </summary>
        private class CustomBsonSerializer : BsonClassMapSerializer<TEntity>
        {
            private readonly Type contextType;

            public CustomBsonSerializer(Type contextType)
                : base(BsonClassMap.RegisterClassMap<TEntity>().Freeze())
            {
                this.contextType = contextType;
            }

            public override TEntity Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var entityType = EntityProxyManager.GetType(contextType, args.NominalType);
                var ser = BsonSerializer.SerializerRegistry.GetSerializer(entityType);
                return (TEntity)ser.Deserialize(context);
            }

            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEntity value)
            {
                base.Serialize(context, args, value);
            }
        }
    }
}
