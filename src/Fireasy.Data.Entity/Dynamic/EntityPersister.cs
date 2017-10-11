// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.QueryBuilder;
using Fireasy.Data.Entity.Validation;
using Fireasy.Data.Extensions;
using Fireasy.Data.Syntax;

namespace Fireasy.Data.Entity.Dynamic
{
    /// <summary>
    /// 一个抽象类，提供对动态实体类型的持久化操作。
    /// </summary>
    public abstract class EntityPersister :
        IEntityPersistentEnvironment,
        IEntityPersistentInstanceContainer,
        IDisposable
    {
        private bool isDisposed;
        private IDatabase database;
        private Type entityType;

        /// <summary>
        /// 初始化 <see cref="EntityPersister"/> 类的新实例。
        /// </summary>
        /// <param name="instanceName">实例名。</param>
        public EntityPersister(string instanceName = null)
        {
            InstanceName = instanceName;
        }

        /// <summary>
        /// 获取当前所使用的 <see cref="IDatabase"/> 对象。
        /// </summary>
        protected IDatabase Database
        {
            get { return database ?? (database = EntityDatabaseFactory.CreateDatabase(InstanceName)); }
        }

        /// <summary>
        /// 获取或设置持久化环境。
        /// </summary>
        public virtual EntityPersistentEnvironment Environment { get; set; }

        /// <summary>
        /// 获取或设置实例名称。
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// 获取实体类型。
        /// </summary>
        /// <returns></returns>
        public Type GetEntityType()
        {
            return entityType ?? (entityType = BuildEntityType());
        }

        /// <summary>
        /// 构造一个动态实体。
        /// </summary>
        /// <returns></returns>
        public object NewEntity()
        {
            return GetEntityType().New();
        }

        /// <summary>
        /// 构造实体类型。
        /// </summary>
        /// <returns></returns>
        protected virtual Type BuildEntityType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        public virtual void Create(object entity)
        {
            var e = entity as IEntity;
            var database = Database;
            ValidationUnity.Validate(e);
            var proxy = new EntityPersisterHelper(database, GetEntityType(), Environment);
            proxy.Create(e);
        }

        /// <summary>
        /// 将实体对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        public virtual void Save(object entity)
        {
            var e = entity as IEntity;
            switch (e.EntityState)
            {
                case EntityState.Modified:
                    Update(e);
                    break;
                case EntityState.Attached:
                    Create(e);
                    break;
                case EntityState.Detached:
                    Remove(e);
                    break;
            }
        }

        /// <summary>
        /// 将一组实体对象的更改保存到库。不会更新实体的其他引用属性。
        /// </summary>
        /// <param name="entities">要保存的实体序列。</param>
        public virtual void Save(IEnumerable<object> entities)
        {
            var proxy = new EntityPersisterHelper(Database, GetEntityType(), Environment);
            proxy.Save(entities);
        }

        /// <summary>
        /// 更新实体对象的修改。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        public virtual void Update(object entity)
        {
            var e = entity as IEntity;
            var database = Database;
            ValidationUnity.Validate(e);
            var proxy = new EntityPersisterHelper(database, GetEntityType(), Environment);
            proxy.Update(e);
        }

        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对数据假删除的支持。</param>
        public virtual void Remove(object entity, bool fake = true)
        {
            var e = entity as IEntity;
            var metadata = EntityMetadataUnity.GetEntityMetadata(GetEntityType());
            var proxy = new EntityPersisterHelper(Database, GetEntityType(), Environment);
            proxy.Remove(e, metadata.DeleteProperty, fake);
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">主键的值。数组的长度必须与实体所定义的主键相匹配。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对对象假删除的支持。</param>
        public virtual void Remove(object[] primaryValues, bool fake = true)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(GetEntityType());
            var proxy = new EntityPersisterHelper(Database, GetEntityType(), Environment);
            proxy.Remove(primaryValues, metadata.DeleteProperty, fake);
        }

        /// <summary>
        /// 返回满足条件的一组实体对象。
        /// </summary>
        /// <param name="condition">一般的条件语句。</param>
        /// <param name="orderBy">排序语句。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns></returns>
        public virtual IEnumerable<object> Query(string condition, string orderBy, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            var syntax = Database.Provider.GetService<ISyntaxProvider>();
            var query = new EntityQueryBuilder(syntax, Environment, GetEntityType(), parameters).Select().All().From().Where(condition).OrderBy(orderBy);
            return Database.InternalExecuteEnumerable(GetEntityType(), query.ToSqlCommand(), segment, parameters).Cast<object>();
        }

        /// <summary>
        /// 根据自定义的SQL语句查询返回一组动态对象。
        /// </summary>
        /// <param name="queryCommand">查询命令。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns></returns>
        public virtual IEnumerable<object> Query(IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null)
        {
            return Database.ExecuteEnumerable(queryCommand, segment, parameters);
        }

        /// <summary>
        /// 使用主键值查询返回一个实体。
        /// </summary>
        /// <param name="primaryValues">主键的值。数组的长度必须与实体所定义的主键相匹配。</param>
        /// <returns></returns>
        public virtual object First(params object[] primaryValues)
        {
            var proxy = new EntityPersisterHelper(Database, GetEntityType(), Environment);
            return proxy.First(primaryValues, o => o.InitializeInstanceName(InstanceName));
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

            if (database != null)
            {
                database.Dispose();
            }

            isDisposed = true;
        }
    }

    /// <summary>
    /// 指定实体类型的持久化类。
    /// </summary>
    public class AssignedEntityPersister : EntityPersister
    {
        private Type entityType;

        /// <summary>
        /// 使用给定的实体类型初始化 <see cref="AssignedEntityPersister"/> 类的新实例。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        public AssignedEntityPersister(Type entityType)
        {
            this.entityType = entityType;
        }

        /// <summary>
        /// 返回构造函数中指定的实体类型。
        /// </summary>
        /// <returns></returns>
        protected override Type BuildEntityType()
        {
            return entityType;
        }
    }
}