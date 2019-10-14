// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
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
    public sealed class MongoDBRepositoryProvider<TEntity> : IRepositoryProvider<TEntity> where TEntity : class, IEntity
    {
        private MongoCollection<TEntity> collection;

        public MongoDBRepositoryProvider(IContextService service)
        {
            var context = service.InitializeContext;
            var connectionString = context.ConnectionString;
            var serverName = connectionString.Properties.TryGetValue("server");
            var database = connectionString.Properties.TryGetValue("database");
            if (string.IsNullOrEmpty(database))
            {
                database = "admin";
            }

            var client = new MongoClient(serverName);
            var server = client.GetServer();
            var db = server.GetDatabase(database);
            var metadata = EntityMetadataUnity.GetEntityMetadata(typeof(TEntity));
            var collectionSettings = new MongoCollectionSettings { AssignIdOnInsert = false };
            collection = db.GetCollection<TEntity>(metadata.TableName, collectionSettings);

            BsonSerializer.RegisterSerializer(new CustomBsonSerializer());

            var provider = new MongoQueryProvider(collection);
            QueryProvider = provider;
            Queryable = new MongoQueryable<TEntity>(provider);
        }

        public IQueryable Queryable { get; private set; }

        public IQueryProvider QueryProvider { get; private set; }

        IQueryable IRepositoryProvider.Queryable => Queryable;

        IQueryProvider IRepositoryProvider.QueryProvider => QueryProvider;

        public async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = collection.Insert(entity);
            return (int)result.DocumentsAffected;
        }

        public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = collection.Save(entity);
            return (int)result.DocumentsAffected;
        }

        public async Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            collection.InsertBatch(entities);
        }

        public async Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            var predicate = BuildPrimaryExpression((p, i) => entity.GetValue(p).GetValue());
            var query = Query<TEntity>.Where(predicate);
            var result = collection.Remove(query);
            return (int)result.DocumentsAffected;
        }

        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            var query = Query<TEntity>.Where(predicate);
            var result = collection.Remove(query);
            return (int)result.DocumentsAffected;
        }

        public async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = Query<TEntity>.Where(predicate);
            var update = GetUpdateBuilder(entity);
            var result = collection.Update(query, update);
            return (int)result.DocumentsAffected;
        }

        public async Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, System.Linq.Expressions.Expression<System.Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<System.Func<IRepository<TEntity>, TEntity, int>> fnOperation, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            var predicate = BuildPrimaryExpression((p, i) => PropertyValue.IsEmpty(primaryValues[i]) ? null : primaryValues[i].GetValue());
            var query = Query<TEntity>.Where(predicate);
            var result = collection.Remove(query);
            return (int)result.DocumentsAffected;
        }

        public async Task<TEntity> GetAsync(PropertyValue[] primaryValues, CancellationToken cancellationToken = default)
        {
            var predicate = BuildPrimaryExpression((p, i) => PropertyValue.IsEmpty(primaryValues[i]) ? null : primaryValues[i].GetValue());
            var query = Query<TEntity>.Where(predicate);
            return collection.FindOne(query);
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

        IRepository IRepositoryProvider.CreateRepository(EntityContextOptions options)
        {
            return new EntityRepository<TEntity>(this, options);
        }

        private class CustomBsonSerializer : BsonClassMapSerializer<TEntity>
        {
            public CustomBsonSerializer()
                : base (BsonClassMap.RegisterClassMap<TEntity>().Freeze())
            {
            }

            public override TEntity Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var entityType = EntityProxyManager.GetType(args.NominalType);
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
