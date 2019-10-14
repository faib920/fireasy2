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
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 提供树实体类型的仓储。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityTreeRepository<TEntity> : ITreeRepository<TEntity>, IQueryProviderAware where TEntity : class, IEntity
    {
        private IRepository<TEntity> repository;
        private EntityMetadata metadata;
        private EntityTreeMetadata metaTree;
        private Type entityType;
        private ISyntaxProvider syntax;
        private IDatabase database;
        private EntityContextOptions options;

        /// <summary>
        /// 初始化 <see cref="EntityTreeRepository{TEntity}"/> 类的新实例。
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="service"></param>
        public EntityTreeRepository(IRepository<TEntity> repository, IContextService service)
        {
            this.repository = repository;
            entityType = typeof(TEntity);
            metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            metaTree = metadata.EntityTree;
            database = service.Database;
            syntax = database.Provider.GetService<ISyntaxProvider>();
            options = service.InitializeContext.Options;
        }

        IQueryProvider IQueryProviderAware.Provider
        {
            get { return repository.Provider; }
        }

        /// <summary>
        /// 将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        public virtual void Insert(TEntity entity, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null)
        {
            InsertAsync(entity, referEntity, position, isolation);
        }

        /// <summary>
        /// 异步的，将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public virtual async Task InsertAsync(TEntity entity, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (referEntity == null)
            {
                var arg = CreateUpdatingArgument(entity);

                //获得新节点的Order值
                arg.NewValue.Order = GetNewOrderNumber(null, EntityTreePosition.Children, 0, isolation);
                arg.NewValue.Level = 1;

                //生成新的InnerID
                arg.NewValue.FullName = arg.OldValue.Name;
                arg.NewValue.InnerId = GenerateInnerId(string.Empty, arg.NewValue.Order, EntityTreePosition.Children);
                UpdateEntityByArgument(entity, arg);

                await repository.InsertAsync(entity);
            }

            var arg1 = CreateUpdatingArgument(entity);
            var arg2 = CreateUpdatingArgument(referEntity);

            var keyId = arg2.OldValue.InnerId;

            //获得新节点的Order值
            arg1.NewValue.Order = GetNewOrderNumber(arg2.OldValue, position, 0, isolation);

            //获得参照节点的级别
            arg1.NewValue.Level = arg2.OldValue.Level;

            //如果插入为孩子，级别则+1
            if (position == EntityTreePosition.Children)
            {
                arg1.NewValue.Level += 1;
            }

            //生成新的InnerID
            arg1.NewValue.InnerId = GenerateInnerId(keyId, arg1.NewValue.Order, position);
            arg1.NewValue.FullName = GenerateFullName(arg1, arg2, position);

            UpdateEntityByArgument(entity, arg1);

            try
            {
                await repository.InsertAsync(entity);
            }
            catch (Exception ex)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityInsert), ex);
            }
        }

        /// <summary>
        /// 将一组实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entities">插入的实体集。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        public virtual void BatchInsert(IEnumerable<TEntity> entities, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null)
        {
            BatchInsertAsync(entities, referEntity, position, isolation);
        }

        /// <summary>
        /// 异步的，将一组实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entities">插入的实体集。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public virtual async Task BatchInsertAsync(IEnumerable<TEntity> entities, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default)
        {
            if (referEntity == null)
            {
                var orderNo1 = GetNewOrderNumber(null, EntityTreePosition.Children, 0, isolation);

                foreach (var entity in entities)
                {
                    var arg = CreateUpdatingArgument(entity);
                    //获得新节点的Order值
                    arg.NewValue.Order = orderNo1++;
                    arg.NewValue.Level = 1;

                    //生成新的InnerID
                    arg.NewValue.FullName = arg.OldValue.Name;
                    arg.NewValue.InnerId = GenerateInnerId(string.Empty, arg.NewValue.Order, EntityTreePosition.Children);

                    UpdateEntityByArgument(entity, arg);
                }

                await repository.BatchAsync(entities, (u, s) => u.Insert(s));
                return;
            }

            var arg2 = CreateUpdatingArgument(referEntity);

            var keyId = arg2.OldValue.InnerId;
            var orderNo = GetNewOrderNumber(arg2.OldValue, position, 0, isolation);

            foreach (var entity in entities)
            {
                var arg1 = CreateUpdatingArgument(entity);

                //获得新节点的Order值
                arg1.NewValue.Order = orderNo++;

                //获得参照节点的级别
                arg1.NewValue.Level = arg2.OldValue.Level;

                //如果插入为孩子，级别则+1
                if (position == EntityTreePosition.Children)
                {
                    arg1.NewValue.Level += 1;
                }

                //生成新的InnerID
                arg1.NewValue.InnerId = GenerateInnerId(keyId, arg1.NewValue.Order, position);
                arg1.NewValue.FullName = GenerateFullName(arg1, arg2, position);

                UpdateEntityByArgument(entity, arg1);
            }

            try
            {
                repository.Batch(entities, (u, s) => u.Insert(s));
            }
            catch (Exception ex)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityInsert), ex);
            }
        }

        /// <summary>
        /// 将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        public virtual void Move(TEntity entity, TEntity referEntity, EntityTreePosition? position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null)
        {
            MoveAsync(entity, referEntity, position, isolation);
        }

        /// <summary>
        /// 异步的，将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public virtual async Task MoveAsync(TEntity entity, TEntity referEntity, EntityTreePosition? position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (referEntity != null && position == null)
            {
                await UpdateCurrentAsync(entity, isolation, cancellationToken);
                return;
            }

            AttachRequiredProperties(entity);

            if (!CheckMovable(entity, referEntity))
            {
                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityMoveWildly), null);
            }

            if (entity.Equals(referEntity) ||
                (position != null && !CheckNeedMove(entity, referEntity, (EntityTreePosition)position)))
            {
                await UpdateCurrentAsync(entity, isolation, cancellationToken);
                return;
            }

            try
            {
                database.BeginTransaction();

                var arg1 = CreateUpdatingArgument(entity);

                //移到根节点
                if (referEntity == null)
                {
                    await UpdateMoveToRootAsync(entity, arg1, isolation, cancellationToken);
                }
                else
                {
                    var arg2 = CreateUpdatingArgument(referEntity);

                    if (position == EntityTreePosition.Children)
                    {
                        await UpdateMoveAsChildrenAsync(entity, referEntity, arg1, arg2, isolation, cancellationToken);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                database.CommitTransaction();
            }
            catch (Exception ex)
            {
                database.RollbackTransaction();

                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityMove), ex);
            }
        }

        /// <summary>
        /// 判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public virtual bool HasChildren(TEntity entity, Expression<Func<TEntity, bool>> predicate = null)
        {
            return HasChildrenAsync(entity, predicate).Result;
        }

        /// <summary>
        /// 异步的，判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public virtual async Task<bool> HasChildrenAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var query = (IQueryable)QueryHelper.CreateQuery<TEntity>(repository.Provider, predicate);
            var mthCount = typeof(Enumerable).GetMethods().FirstOrDefault(s => s.Name == "Count" && s.GetParameters().Length == 2);
            mthCount = mthCount.MakeGenericMethod(typeof(TEntity));

            var expression = TreeExpressionBuilder.BuildHasChildrenExpression(metaTree, entity, predicate);
            expression = Expression.Call(null, mthCount, query.Expression, expression);
            return await ((IAsyncQueryProvider)repository.Provider).ExecuteAsync<int>(expression, cancellationToken) > 0;
        }

        /// <summary>
        /// 查询指定实体的孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="recurrence">是否递归出所有孩子。</param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> QueryChildren(TEntity entity, Expression<Func<TEntity, bool>> predicate = null, bool recurrence = false)
        {
            var expression = TreeExpressionBuilder.BuildQueryChildrenExpression(metaTree, entity, predicate, recurrence);

            var querable = QueryHelper.CreateQuery<TEntity>(repository.Provider, expression);

            var orderExp = TreeExpressionBuilder.BuildOrderByExpression<TEntity>(metaTree, querable.Expression);

            return repository.Provider.CreateQuery<TEntity>(orderExp);
        }

        /// <summary>
        /// 递归返回实体的父亲实体。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public IQueryable<TEntity> RecurrenceParent(TEntity entity, Expression<Func<TEntity, bool>> predicate = null)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            var keyId = (string)entity.GetValue(metaTree.InnerSign);

            var parents = new List<string>();
            while ((keyId = keyId.Substring(0, keyId.Length - metaTree.SignLength)).Length > 0)
            {
                parents.Add(keyId);
            }

            var expression = TreeExpressionBuilder.BuildGetByInnerIdExpression<TEntity>(metaTree, predicate, parents);

            var querable = QueryHelper.CreateQuery<TEntity>(repository.Provider, expression);

            var orderExp = TreeExpressionBuilder.BuildOrderByLengthDescExpression<TEntity>(metaTree, querable.Expression);

            return repository.Provider.CreateQuery<TEntity>(orderExp);
        }

        private void AttachRequiredProperties(TEntity entity)
        {
            var pkValues = new List<PropertyValue>();
            foreach (var pkProperty in PropertyUnity.GetPrimaryProperties(typeof(TEntity)))
            {
                pkValues.Add(entity.GetValue(pkProperty));
            }

            var oldEntity = repository.Get(pkValues.ToArray());
            if (oldEntity == null)
            {
                return;
            }

            if (metaTree.InnerSign != null && !entity.IsModified(metaTree.InnerSign.Name))
            {
                entity.InitializeValue(metaTree.InnerSign, oldEntity.GetValue(metaTree.InnerSign));
            }

            if (metaTree.Name != null && !entity.IsModified(metaTree.Name.Name))
            {
                entity.InitializeValue(metaTree.Name, oldEntity.GetValue(metaTree.Name));
            }

            if (metaTree.FullName != null && !entity.IsModified(metaTree.FullName.Name))
            {
                entity.InitializeValue(metaTree.FullName, oldEntity.GetValue(metaTree.FullName));
            }

            if (metaTree.Order != null && !entity.IsModified(metaTree.Order.Name))
            {
                entity.InitializeValue(metaTree.Order, oldEntity.GetValue(metaTree.Order));
            }

            if (metaTree.Level != null && !entity.IsModified(metaTree.Level.Name))
            {
                entity.InitializeValue(metaTree.Level, oldEntity.GetValue(metaTree.Level));
            }
        }

        /// <summary>
        /// 取参照实体的最大order值。
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="mode"></param>
        /// <param name="offset"></param>
        /// <param name="isolation"></param>
        /// <returns></returns>
        private int GetNewOrderNumber(EntityTreeUpdatingBag bag, EntityTreePosition mode, int offset = 0, Expression<Func<TEntity>> isolation = null)
        {
            if (bag == null)
            {
                return GetNewOrderNumber(isolation);
            }

            switch (mode)
            {
                case EntityTreePosition.Children:
                    var sql = string.Format("SELECT MAX({0}) FROM {1} WHERE {2} LIKE {3}",
                        GetOrderExpression(),
                        DbUtility.FormatByQuote(syntax, metadata.TableName),
                        QuoteColumn(metaTree.InnerSign),
                        syntax.FormatParameter("pm"));
                    var innerId = bag.InnerId;

                    if (isolation != null)
                    {
                        var condition = IsolationConditionBuilder.Build(isolation);
                        if (!string.IsNullOrEmpty(condition))
                        {
                            sql += " AND " + condition;
                        }
                    }

                    var parameters = new ParameterCollection { { "pm", innerId + new string('_', metaTree.SignLength) } };
                    return database.ExecuteScalar((SqlCommand)sql, parameters).To<int>() + 1 + offset;
                case EntityTreePosition.Before:
                    return bag.Order + offset;
                case EntityTreePosition.After:
                    return bag.Order + 1 + offset;
            }

            return 0;
        }

        /// <summary>
        /// 取顶层的最大order值。
        /// </summary>
        /// <returns></returns>
        private int GetNewOrderNumber(Expression<Func<TEntity>> isolation = null)
        {
            var sql = string.Format("SELECT MAX({0}) FROM {1} WHERE {2} = {3}",
                GetOrderExpression(),
                DbUtility.FormatByQuote(syntax, metadata.TableName),
                syntax.String.Length(QuoteColumn(metaTree.InnerSign)),
                metaTree.SignLength);

            if (isolation != null)
            {
                var condition = IsolationConditionBuilder.Build(isolation);
                if (!string.IsNullOrEmpty(condition))
                {
                    sql += " AND " + condition;
                }
            }

            return database.ExecuteScalar((SqlCommand)sql).To<int>() + 1;
        }

        /// <summary>
        /// 判断是否可以移动。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="referEntity"></param>
        /// <returns></returns>
        private bool CheckMovable(TEntity entity, TEntity referEntity)
        {
            if (referEntity == null)
            {
                return true;
            }

            var bag1 = ParseEntityData(entity);
            var bag2 = ParseEntityData(referEntity);

            if (bag2.Level > bag1.Level &&
                bag2.InnerId.StartsWith(bag1.InnerId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否需要移动。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="referEntity"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool CheckNeedMove(TEntity entity, TEntity referEntity, EntityTreePosition position)
        {
            var bag1 = ParseEntityData(entity);
            if (referEntity == null)
            {
                return bag1.Level != 1;
            }

            var bag2 = ParseEntityData(referEntity);
            var isBrotherly = IsBrotherly(bag1, bag2);
            if (isBrotherly)
            {
                //判断是否需要移动
                if (position == EntityTreePosition.After && bag1.Order - bag2.Order == 1)
                {
                    return false;
                }

                if (position == EntityTreePosition.Before && bag2.Order - bag1.Order == 1)
                {
                    return false;
                }
            }
            //本来就属于它的孩子
            else if (position == EntityTreePosition.Children &&
                bag1.InnerId.StartsWith(bag2.InnerId) &&
                bag1.InnerId.Length == bag2.InnerId.Length + metaTree.SignLength)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 使用 <see cref="EntityTreeUpdatingBag"/> 判断是否具有兄弟关系。
        /// </summary>
        /// <param name="bag1"></param>
        /// <param name="bag2"></param>
        /// <returns></returns>
        private bool IsBrotherly(EntityTreeUpdatingBag bag1, EntityTreeUpdatingBag bag2)
        {
            if ((bag1.InnerId.Length != bag2.InnerId.Length) || bag2.InnerId.Length < metaTree.SignLength)
            {
                return false;
            }

            return bag1.InnerId.Substring(0, bag1.InnerId.Length - metaTree.SignLength)
                .Equals(bag2.InnerId.Substring(0, bag2.InnerId.Length - metaTree.SignLength));
        }

        /// <summary>
        /// 获取Order的表达式。
        /// </summary>
        /// <returns></returns>
        private string GetOrderExpression()
        {
            //如果Order没有指定，则取InnerId的后N位转成数字
            if (metaTree.Order == null)
            {
                var field = DbUtility.FormatByQuote(syntax, metaTree.InnerSign.Info.FieldName);
                return syntax.Convert(
                    syntax.String.Substring(field, syntax.String.Length(field) + " + 1 - " + metaTree.SignLength,
                    metaTree.SignLength), DbType.Int32);
            }

            return DbUtility.FormatByQuote(syntax, metaTree.Order.Info.FieldName);
        }

        /// <summary>
        /// 获取Level的表达式。
        /// </summary>
        /// <returns></returns>
        private string GetLevelExpression()
        {
            //如果Level没有指定，则取InnerId的长度除以N
            if (metaTree.Level == null)
            {
                return syntax.String.Length(DbUtility.FormatByQuote(syntax, metaTree.InnerSign.Info.FieldName)) + " / " + metaTree.SignLength;
            }

            return DbUtility.FormatByQuote(syntax, metaTree.Order.Info.FieldName);
        }

        /// <summary>
        /// 生成内码。
        /// </summary>
        /// <param name="keyId"></param>
        /// <param name="order"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private string GenerateInnerId(string keyId, PropertyValue order, EntityTreePosition position)
        {
            var sOrder = order.ToString();
            if (metaTree.SignLength - sOrder.Length < 0)
            {
                throw new EntityTreeCodeOutOfRangeException(SR.GetString(SRKind.TreeCodeOutOfRange, metaTree.SignLength));
            }

            return position == EntityTreePosition.Children || keyId.Length < metaTree.SignLength ?
                keyId + new string('0', metaTree.SignLength - sOrder.Length) + sOrder :
                GetPreviousKey(keyId) + new string('0', metaTree.SignLength - sOrder.Length) + sOrder;
        }

        /// <summary>
        /// 取前面n级内码
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetPreviousKey(string key, int index = 1)
        {
            if (key.Length - (metaTree.SignLength * index) <= 0)
            {
                return string.Empty;
            }

            return key.Length < metaTree.SignLength ? key : key.Substring(0, key.Length - (metaTree.SignLength * index));
        }

        /// <summary>
        /// 取前面一级的全名
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private string GetPreviousFullName(string fullName)
        {
            var index = fullName.LastIndexOf(metaTree.NameSeparator);
            return index != -1 ? fullName.Substring(0, index) : string.Empty;
        }

        /// <summary>
        /// 获取右边的全名
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private string GetRightFullName(string fullName, int level)
        {
            var s = fullName;
            for (var i = 0; i < level; i++)
            {
                var index = s.IndexOf(metaTree.NameSeparator);
                if (index == -1)
                {
                    break;
                }

                s = s.Substring(index + 1);
            }

            return s;
        }

        /// <summary>
        /// 生成新的全名，以参考节点的全名作为基础
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private string GenerateFullName(EntityTreeUpfydatingArgument arg1, EntityTreeUpfydatingArgument arg2, EntityTreePosition position)
        {
            var fullName = string.Empty;
            if (metaTree.Name == null || metaTree.FullName == null)
            {
                return null;
            }

            //没有参考节点，则使用名称
            if (arg2 == null)
            {
                fullName = arg1.NewValue.Name;
            }
            else
            {
                //获得参考节点的全名
                fullName = arg2.NewValue.FullName;
                if (position != EntityTreePosition.Children)
                {
                    //取前面一级的全名
                    fullName = GetPreviousFullName(fullName);
                }

                //拼接上当前的名称
                fullName = string.Format("{0}{1}{2}", fullName, metaTree.NameSeparator, arg1.NewValue.Name);
            }

            return fullName;
        }

        /// <summary>
        /// 获取孩子、孙子、重孙...。
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="isolation"></param>
        /// <returns></returns>
        private IEnumerable<TEntity> GetChildren(EntityTreeUpfydatingArgument argument, Expression<Func<TEntity>> isolation = null)
        {
            var expression = TreeExpressionBuilder.BuildGetChildrenExpression<TEntity>(metaTree, argument.OldValue.InnerId);

            var querable = QueryHelper.CreateQuery<TEntity>(repository.Provider, expression);
            expression = TreeExpressionBuilder.AddIsolationExpression<TEntity>(querable.Expression, isolation);
            expression = TreeExpressionBuilder.AddUseableSelectExpression<TEntity>(metaTree, expression);

            return repository.Provider.CreateQuery<TEntity>(expression).ToList();
        }

        /// <summary>
        /// 获取兄弟及他们的孩子。
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="includeCurrent">是否包含当 <paramref name="argument"/>，当在它前面插入时，需要包含它。</param>
        /// <param name="excludeArg">要排除的实体。</param>
        /// <param name="isTop">是否遇到要排除的实体就终止。</param>
        /// <param name="isolation"></param>
        /// <returns></returns>
        private IEnumerable<TEntity> GetBrothersAndChildren(EntityTreeUpfydatingArgument argument, bool includeCurrent, EntityTreeUpfydatingArgument excludeArg, bool isTop = false, Expression<Func<TEntity>> isolation = null)
        {
            var keyId = argument.OldValue.InnerId;
            var order = argument.OldValue.Order;
            var level = argument.OldValue.Level;
            var parameters = new ParameterCollection();
            var m = EntityMetadataUnity.GetEntityMetadata(entityType);

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            var assert = new AssertFlag();
            foreach (var property in GetUseableProperties())
            {
                if (property == null)
                {
                    continue;
                }

                if (!assert.AssertTrue())
                {
                    sb.Append(", ");
                }

                sb.AppendFormat("t.{0} {0}", QuoteColumn(property));
            }

            sb.AppendFormat(" FROM {0} t", GetTableName());
            sb.AppendFormat(" JOIN (SELECT f.{0} {0} FROM {1} f", QuoteColumn(metaTree.InnerSign), GetTableName());

            sb.AppendFormat(" WHERE {0} LIKE {1} AND {5} = {6} AND {2} {4} {3}", QuoteColumn(metaTree.InnerSign), syntax.FormatParameter("pn"),
                GetOrderExpression(), order, includeCurrent ? ">=" : ">", GetLevelExpression(), level);

            if (m.DeleteProperty != null)
            {
                sb.AppendFormat(" AND {0} = {1}", QuoteColumn(m.DeleteProperty), 0);
            }

            if (excludeArg != null)
            {
                var excludeId = excludeArg.OldValue.InnerId;
                sb.AppendFormat(" AND NOT ({0} LIKE {1})", QuoteColumn(metaTree.InnerSign), syntax.FormatParameter("px"));
                parameters.Add("px", excludeId + "%");

                if (isTop)
                {
                    sb.AppendFormat(" AND {0} < {1}", QuoteColumn(metaTree.InnerSign), syntax.FormatParameter("px1"));
                    parameters.Add("px1", excludeId);
                }
            }

            if (!includeCurrent)
            {
                sb.AppendFormat(" AND {0} <> {1}", QuoteColumn(metaTree.InnerSign), syntax.FormatParameter("pm"));
                parameters.Add("pm", keyId);
            }

            var conIsolation = string.Empty;
            if (isolation != null)
            {
                conIsolation = IsolationConditionBuilder.Build(isolation);
                if (!string.IsNullOrEmpty(conIsolation))
                {
                    sb.AppendFormat(" AND {0}", conIsolation);
                }
            }

            sb.AppendFormat(") f ON t.{0} LIKE {1}", QuoteColumn(metaTree.InnerSign), syntax.String.Concat("f." + QuoteColumn(metaTree.InnerSign), "'%'"));

            if (!string.IsNullOrEmpty(conIsolation))
            {
                sb.AppendFormat(" WHERE {0}", conIsolation);
            }

            sb.AppendFormat("ORDER BY {0}", QuoteColumn(metaTree.InnerSign));

            keyId = GetPreviousKey(keyId) + "_%";
            parameters.Add("pn", keyId);

            return database.ExecuteEnumerable((SqlCommand)sb.ToString(), parameters: parameters, rowMapper: new EntityRowMapper<TEntity>()).ToList();
        }

        /// <summary>
        /// 更新所有孩子的全名。
        /// </summary>
        /// <param name="current">当前的实体对象。</param>
        /// <param name="children">要更新的子实体对象。</param>
        /// <param name="fuleName">当前实体对象的全名。</param>
        private void UpdateChildrenFullName(TEntity current, IEnumerable<TEntity> children, string fuleName)
        {
            var list = new List<EntityTreeUpdatingBag>();
            foreach (var entity in children)
            {
                var arg = CreateUpdatingArgument(entity);

                var rowInnerId = arg.NewValue.InnerId;

                //取得上一级编码，然后找到父的全名
                var prevRowInnerId = GetPreviousKey(rowInnerId);
                var parentRow = list.FirstOrDefault(s => s.InnerId == prevRowInnerId);
                var newFullName = parentRow == null ? fuleName :
                    parentRow.FullName;

                //父全名+分隔+当前元素的名称
                newFullName = string.Format("{0}{1}{2}", newFullName, metaTree.NameSeparator, arg.OldValue.Name);

                arg.NewValue.FullName = newFullName;

                UpdateEntityByArgument(entity, arg);

                list.Add(arg.NewValue);
            }

            list.Clear();
        }

        private void UpdateChildren(TEntity current, IEnumerable<TEntity> entities, EntityTreeUpfydatingArgument argument)
        {
            foreach (var entity in entities)
            {
                var arg = CreateUpdatingArgument(entity);

                arg.NewValue.Level = argument.NewValue.Level + arg.OldValue.Level - argument.OldValue.Level;
                arg.NewValue.InnerId = argument.NewValue.InnerId + arg.OldValue.InnerId.Substring(argument.OldValue.Level * metaTree.SignLength);

                if (metaTree.FullName != null && !string.IsNullOrEmpty(argument.NewValue.FullName))
                {
                    arg.NewValue.FullName = argument.NewValue.FullName + metaTree.NameSeparator + GetRightFullName(arg.OldValue.FullName, argument.OldValue.Level);
                }

                UpdateEntityByArgument(entity, arg);
            }
        }

        /// <summary>
        /// 将兄弟和孩子的前N级编码前移或后移，N取决于currentInnerId的长度，如果currentInnerId为空，则取数据表中的第一行数据的InnerID
        /// </summary>
        /// <param name="current">当前的实体对象。</param>
        /// <param name="entities">要移动的子实体对象。</param>
        /// <param name="currentInnerId">当前的内码。</param>
        /// <param name="position">移动的偏离位置。</param>
        private void UpdateBrothersAndChildren(TEntity current, IEnumerable<TEntity> entities, string currentInnerId, int position)
        {
            var dictionary = new Dictionary<string, EntityTreeUpfydatingArgument>();

            foreach (var entity in entities)
            {
                var arg = CreateUpdatingArgument(entity);

                var rowInnerId = arg.OldValue.InnerId;
                if (string.IsNullOrEmpty(currentInnerId))
                {
                    currentInnerId = rowInnerId;
                }

                dictionary.TryAdd(arg.OldValue.InnerId, arg);

                var prevRowInnerId = GetPreviousKey(rowInnerId);

                //找父节点
                EntityTreeUpfydatingArgument parArg;

                if (dictionary.TryGetValue(prevRowInnerId, out parArg))
                {
                    prevRowInnerId = parArg.NewValue.InnerId;
                }

                if (currentInnerId.Length == rowInnerId.Length)
                {
                    arg.NewValue.Order += position;
                }

                var sorder = arg.NewValue.Order.ToString();
                arg.NewValue.InnerId = prevRowInnerId + new string('0', metaTree.SignLength - sorder.Length) + sorder;

                UpdateEntityByArgument(entity, arg);
            }
        }

        /// <summary>
        /// 节点移动到根目录下，相关节点的更新。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="arg"></param>
        /// <param name="isolation"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        private async Task UpdateMoveToRootAsync(TEntity current, EntityTreeUpfydatingArgument arg, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default)
        {
            //获得新节点的Order值
            var newOrder = GetNewOrderNumber(null, EntityTreePosition.Children, isolation: isolation);

            //获取它的兄弟及其孩子
            var brothers = GetBrothersAndChildren(arg, false, null, isolation: isolation);

            //获取它的孩子
            var children = GetChildren(arg, isolation);

            //生成新的InnerID
            var currentInnerId = GenerateInnerId(string.Empty, newOrder, EntityTreePosition.Children);

            //全名即为名称
            if (metaTree.FullName != null && metaTree.Name != null)
            {
                arg.NewValue.FullName = arg.OldValue.Name;
            }

            arg.NewValue.InnerId = currentInnerId;
            arg.NewValue.Level = 1;
            arg.NewValue.Order = newOrder;

            UpdateEntityByArgument(current, arg);

            //兄弟及其孩子要上移一个单位
            UpdateBrothersAndChildren(current, brothers, string.Empty, -1);

            //它的孩子要移到根节点下
            UpdateChildren(current, children, arg);

            SetNameNotModified(new[] { current });
            SetNameNotModified(brothers);
            SetNameNotModified(children);

            await repository.UpdateAsync(current);

            await repository.BatchAsync(brothers, (u, s) => u.Update(s));
            await repository.BatchAsync(children, (u, s) => u.Update(s));
        }

        /// <summary>
        /// 将子节点移动到另一个子节点的下面。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="referEntity"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="isolation"></param>
        private async Task UpdateMoveAsChildrenAsync(TEntity current, TEntity referEntity, EntityTreeUpfydatingArgument arg1, EntityTreeUpfydatingArgument arg2, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default)
        {
            //获取要移动节点的兄弟及其孩子
            var brothers = GetBrothersAndChildren(arg1, false, null, isolation: isolation);

            //获取要移动的节点的孩子
            var children = GetChildren(arg1, isolation);

            //兄弟及其孩子要下移一个单位
            UpdateBrothersAndChildren(current, brothers, arg1.OldValue.InnerId, -1);

            //获得新节点的Order值
            arg1.NewValue.Order = GetNewOrderNumber(arg2.OldValue, EntityTreePosition.Children, isolation: isolation);

            var modify = IsInList(referEntity, brothers);
            if (modify != null)
            {
                arg2 = CreateUpdatingArgument(modify);
            }

            var keyId = arg2.OldValue.InnerId;

            //获得参照节点的级别
            arg1.NewValue.Level = arg2.OldValue.Level + 1;

            //生成新的InnerID
            arg1.NewValue.InnerId = GenerateInnerId(keyId, arg1.NewValue.Order, EntityTreePosition.Children);
            arg1.NewValue.FullName = GenerateFullName(arg1, arg2, EntityTreePosition.Children);

            //更新要移动的节点的孩子
            UpdateChildren(current, children, arg1);
            UpdateEntityByArgument(current, arg1);

            SetNameNotModified(new[] { current });
            SetNameNotModified(brothers);
            SetNameNotModified(children);

            await repository.UpdateAsync(current, cancellationToken);

            await repository.BatchAsync(brothers, (u, s) => u.Update(s), cancellationToken);
            await repository.BatchAsync(children, (u, s) => u.Update(s), cancellationToken);
        }

        /// <summary>
        /// 更新当前节点。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="isolation"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        private async Task UpdateCurrentAsync(TEntity current, Expression<Func<TEntity>> isolation, CancellationToken cancellationToken = default)
        {
            if (metaTree.FullName != null && current.IsModified(metaTree.Name.Name))
            {
                var arg = CreateUpdatingArgument(current);

                var fullName = GetPreviousFullName(arg.OldValue.FullName);

                fullName = string.IsNullOrEmpty(fullName) ?
                    arg.NewValue.Name : string.Format("{0}{1}{2}", fullName, metaTree.NameSeparator, arg.NewValue.Name);

                arg.NewValue.FullName = fullName;

                var children = GetChildren(arg, isolation);

                UpdateChildren(current, children, arg);
                UpdateEntityByArgument(current, arg);

                SetNameNotModified(children);

                await repository.BatchAsync(children, (u, s) => u.Update(s));
            }

            SetNameNotModified(new[] { current });

            await repository.UpdateAsync(current);
        }

        /// <summary>
        /// 判断实体是否在指定的集合中。如果它在集合中，它有可能被修改。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        private TEntity IsInList(TEntity entity, IEnumerable<TEntity> entities)
        {
            return entities.FirstOrDefault(item => item.Equals(entity));
        }

        /// <summary>
        /// 设置不要更新 Name。
        /// </summary>
        /// <param name="eitities"></param>
        private void SetNameNotModified(IEnumerable<TEntity> eitities)
        {
            if (metaTree.Name != null)
            {
                eitities.ForEach(s => s.NotifyModified(metaTree.Name.Name, false));
            }
        }

        /// <summary>
        /// 获取属性的对应的字段表达式，并在前后加上标识符。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private string QuoteColumn(IProperty property)
        {
            return DbUtility.FormatByQuote(syntax, property.Info.FieldName);
        }

        /// <summary>
        /// 获取实体类所对应的表的名称。
        /// </summary>
        /// <returns></returns>
        private string GetTableName()
        {
            return DbUtility.FormatByQuote(syntax, metadata.TableName);
        }

        /// <summary>
        /// 获取可以组织到查询里的属性。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IProperty> GetUseableProperties()
        {
            foreach (var pkProperty in PropertyUnity.GetPrimaryProperties(entityType))
            {
                if (pkProperty != metaTree.InnerSign)
                {
                    yield return pkProperty;
                }
            }

            yield return metaTree.InnerSign;
            if (metaTree.Name != null && metaTree.FullName != null)
            {
                yield return metaTree.Name;
            }

            if (metaTree.FullName != null)
            {
                yield return metaTree.FullName;
            }

            if (metaTree.Order != null)
            {
                yield return metaTree.Order;
            }

            if (metaTree.Level != null)
            {
                yield return metaTree.Level;
            }
        }

        /// <summary>
        /// 根据指定的实体创建一个 <see cref="EntityTreeUpfydatingArgument"/> 对象。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private EntityTreeUpfydatingArgument CreateUpdatingArgument(TEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            var value = ParseEntityData(entity);
            return new EntityTreeUpfydatingArgument
            {
                OldValue = value,
                NewValue = value.Clone()
            };
        }

        /// <summary>
        /// 从 <see cref="IEntity"/> 中解析出各个树属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private EntityTreeUpdatingBag ParseEntityData(TEntity entity)
        {
            var data = new EntityTreeUpdatingBag
            {
                InnerId = (string)entity.GetValue(metaTree.InnerSign),
            };

            if (metaTree.Order != null)
            {
                data.Order = (int)entity.GetValue(metaTree.Order);
            }
            else if (!string.IsNullOrEmpty(data.InnerId))
            {
                data.Order = int.Parse(data.InnerId.Right(metaTree.SignLength));
            }

            if (metaTree.Level != null)
            {
                data.Level = (int)entity.GetValue(metaTree.Level);
            }
            else if (!string.IsNullOrEmpty(data.InnerId))
            {
                data.Level = data.InnerId.Length / metaTree.SignLength;
            }

            if (metaTree.Name != null)
            {
                data.Name = (string)entity.GetValue(metaTree.Name);
            }

            if (metaTree.FullName != null)
            {
                data.FullName = (string)entity.GetValue(metaTree.FullName);
            }

            return data;
        }

        /// <summary>
        /// 使用 <see cref="EntityTreeUpfydatingArgument"/> 更新实体属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="argument"></param>
        /// <param name="force">是否强制修改。</param>
        private void UpdateEntityByArgument(TEntity entity, EntityTreeUpfydatingArgument argument, bool force = false)
        {
            //force强制修改属性
            if (force || argument.OldValue.InnerId != argument.NewValue.InnerId)
            {
                entity.SetValue(metaTree.InnerSign, argument.NewValue.InnerId);
            }

            if (metaTree.Level != null &&
                (force || argument.OldValue.Level != argument.NewValue.Level))
            {
                entity.SetValue(metaTree.Level, argument.NewValue.Level);
            }

            if (metaTree.Order != null &&
                (force || argument.OldValue.Order != argument.NewValue.Order))
            {
                entity.SetValue(metaTree.Order, argument.NewValue.Order);
            }

            if (metaTree.Name != null &&
                (force || argument.OldValue.Name != argument.NewValue.Name))
            {
                entity.SetValue(metaTree.Name, argument.NewValue.Name);
            }

            if (metaTree.FullName != null &&
                (force || argument.OldValue.FullName != argument.NewValue.FullName))
            {
                entity.SetValue(metaTree.FullName, argument.NewValue.FullName);
            }
        }

        #region 实现 ITreeRepository 接口
        void ITreeRepository.Insert(IEntity entity, IEntity referEntity, EntityTreePosition position, Expression isolation)
        {
            Insert((TEntity)entity, (TEntity)referEntity, position, (Expression<Func<TEntity>>)isolation);
        }

        async Task ITreeRepository.InsertAsync(IEntity entity, IEntity referEntity, EntityTreePosition position, Expression isolation, CancellationToken cancellationToken)
        {
            await InsertAsync((TEntity)entity, (TEntity)referEntity, position, (Expression<Func<TEntity>>)isolation, cancellationToken);
        }

        void ITreeRepository.BatchInsert(IEnumerable entities, IEntity referEntity, EntityTreePosition position, Expression isolation)
        {
            BatchInsert(entities.Enumerable<TEntity>(), (TEntity)referEntity, position, (Expression<Func<TEntity>>)isolation);
        }

        async Task ITreeRepository.BatchInsertAsync(IEnumerable entities, IEntity referEntity, EntityTreePosition position, Expression isolation, CancellationToken cancellationToken)
        {
            await BatchInsertAsync(entities.Enumerable<TEntity>(), (TEntity)referEntity, position, (Expression<Func<TEntity>>)isolation, cancellationToken);
        }

        void ITreeRepository.Move(IEntity entity, IEntity referEntity, EntityTreePosition? position, Expression isolation)
        {
            Move((TEntity)entity, (TEntity)referEntity, position, (Expression<Func<TEntity>>)isolation);
        }

        async Task ITreeRepository.MoveAsync(IEntity entity, IEntity referEntity, EntityTreePosition? position, Expression isolation, CancellationToken cancellationToken)
        {
            await MoveAsync((TEntity)entity, (TEntity)referEntity, position, (Expression<Func<TEntity>>)isolation, cancellationToken);
        }

        bool ITreeRepository.HasChildren(IEntity entity, Expression predicate)
        {
            return HasChildren((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate);
        }

        async Task<bool> ITreeRepository.HasChildrenAsync(IEntity entity, Expression predicate, CancellationToken cancellationToken)
        {
            return await HasChildrenAsync((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate, cancellationToken);
        }

        IQueryable ITreeRepository.QueryChildren(IEntity entity, Expression predicate, bool recurrence)
        {
            return QueryChildren((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate, recurrence);
        }

        IQueryable ITreeRepository.RecurrenceParent(IEntity entity, Expression predicate)
        {
            return RecurrenceParent((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate);
        }
        #endregion

        /// <summary>
        /// 数据隔离条件生成器。
        /// </summary>
        private class IsolationConditionBuilder : Common.Linq.Expressions.ExpressionVisitor
        {
            private StringBuilder condition = new StringBuilder();

            public static string Build(Expression expression)
            {
                var builder = new IsolationConditionBuilder();
                builder.Visit(expression);
                return builder.condition.ToString();
            }

            protected override MemberBinding VisitBinding(MemberBinding binding)
            {
                var assign = binding as MemberAssignment;
                if (assign == null)
                {
                    return binding;
                }

                var propertyName = assign.Member.Name;
                var property = PropertyUnity.GetProperty(typeof(TEntity), propertyName);
                if (property != null)
                {
                    if (condition.Length > 0)
                    {
                        condition.Append(" AND ");
                    }

                    var constExp = PartialEvaluator.Eval(assign.Expression) as ConstantExpression;
                    if (constExp.Type.IsStringOrDateTime())
                    {
                        condition.AppendFormat("{0} = '{1}'", property.Info.FieldName, constExp.Value);
                    }
                    else if (constExp.Type.IsEnum)
                    {
                        condition.AppendFormat("{0} = {1}", property.Info.FieldName, (int)constExp.Value);
                    }
                    else
                    {
                        condition.AppendFormat("{0} = {1}", property.Info.FieldName, constExp.Value);
                    }
                }

                return binding;
            }
        }
    }
}
