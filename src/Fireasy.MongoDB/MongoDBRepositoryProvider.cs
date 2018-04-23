// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.MongoDB
{
    public sealed class MongoDBRepositoryProvider<TEntity> : IRepositoryProvider<TEntity> where TEntity : IEntity
    {
        private MongoCollection<TEntity> collection;

        public MongoDBRepositoryProvider(InternalContext context)
        {
            var connectionString = context.Database.ConnectionString;
            var client = new MongoClient((string)connectionString);
            var db = client.GetDatabase(connectionString.Properties.TryGetValue("database"));
            var metadata = EntityMetadataUnity.GetEntityMetadata(typeof(TEntity));
            var collectionSettings = new MongoCollectionSettings { AssignIdOnInsert = false };
            collection = (MongoCollection<TEntity>)db.GetCollection<TEntity>(metadata.TableName, collectionSettings);
            var provider = new MongoQueryProvider(collection);
            QueryProvider = provider;
            Queryable = new MongoQueryable<TEntity>(provider);
        }

        public IQueryable Queryable { get; private set; }

        public IQueryProvider QueryProvider { get; private set; }

        public int Insert(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, System.Action<int> completePercentage = null)
        {
            throw new NotImplementedException();
        }

        public int Delete(TEntity entity, bool logicalDelete = true)
        {
            throw new NotImplementedException();
        }

        public int Delete(object[] primaryValues, bool logicalDelete = true)
        {
            throw new NotImplementedException();
        }

        public TEntity Get(params object[] primaryValues)
        {
            throw new NotImplementedException();
        }

        public int Delete(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true)
        {
            throw new NotImplementedException();
        }

        public int Update(TEntity entity, System.Linq.Expressions.Expression<System.Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public int Update(System.Linq.Expressions.Expression<System.Func<TEntity, TEntity>> calculator, System.Linq.Expressions.Expression<System.Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public int Batch(IEnumerable<TEntity> instances, System.Linq.Expressions.Expression<System.Func<IRepository<TEntity>, TEntity, int>> fnOperation)
        {
            throw new NotImplementedException();
        }

        public int Delete(params PropertyValue[] primaryValues)
        {
            throw new NotImplementedException();
        }

        public int Delete(PropertyValue[] primaryValues, bool logicalDelete = true)
        {
            throw new NotImplementedException();
        }

        public TEntity Get(params PropertyValue[] primaryValues)
        {
            throw new NotImplementedException();
        }
    }
}
