// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.QueryBuilder;
using Fireasy.Data.Extensions;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Syntax;
using Fireasy.Data.Entity.Validation;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体树持久化的辅助类。
    /// </summary>
    public sealed class EntityTreePersistentHelper
    {
        private readonly EntityTreeMetadata metadata;
        private readonly Type entityType;
        private Type entityListType;
        private string tableName;
        private readonly IDatabase database;
        private readonly ISyntaxProvider syntax;
        private readonly EntityPersistentEnvironment environment;

        /// <summary>
        /// 实体树更新时通知客户端程序。
        /// </summary>
        public event EntityTreeUpdatingEventHandler EntityTreeUpdating;

        /// <summary>
        /// 初始化 <see cref="EntityTreePersistentHelper"/> 类的新实例。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="entityType"></param>
        /// <param name="metadata"></param>
        /// <param name="environment"></param>
        public EntityTreePersistentHelper(IDatabase database, Type entityType, EntityTreeMetadata metadata, EntityPersistentEnvironment environment)
        {
            CheckEntityTreeMetadata(entityType, metadata);
            this.metadata = metadata;
            this.database = database;
            syntax = database.Provider.GetService<ISyntaxProvider>();
            this.entityType = entityType;
            this.environment = environment;
        }

        /// <summary>
        /// 初始化 <see cref="EntityTreePersistentHelper"/> 类的新实例。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="entityType"></param>
        /// <param name="attribute"></param>
        /// <param name="environment"></param>
        public EntityTreePersistentHelper(IDatabase database, Type entityType, EntityTreeMappingAttribute attribute, EntityPersistentEnvironment environment)
            : this(database, entityType, EntityTreeMetadata.CreateMetadata(entityType, attribute), environment)
        {
        }

        /// <summary>
        /// 将一个新的实体对象持久化。
        /// </summary>
        /// <param name="entity">要持久化的实体对象。</param>
        public void Create(IEntity entity)
        {
            try
            {
                var arg = CreateUpdatingArgument(entity);

                //获得新节点的Order值
                arg.NewValue.Order = GetNewOrderNumber(null, EntityTreePosition.Children);
                arg.NewValue.Level = 1;

                //生成新的InnerID
                arg.NewValue.FullName = arg.OldValue.Name;
                arg.NewValue.InnerId = GenerateInnerId(string.Empty, arg.NewValue.Order, EntityTreePosition.Children);
                UpdateEntityByArgument(entity, arg);

                var helper = CreatePersisterHelper();
                helper.Create(entity);
            }
            catch (Exception ex)
            {
                throw new EntityTreePersistentException(SR.GetString(SRKind.FailInEntityCreate), ex);
            }
        }

        /// <summary>
        /// 更新实体对象的修改。如果已经定义了 FullName 则会更新当前对象及相关的子实体对象 的 FullName 属性。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        public void Update(IEntity entity)
        {
            if (entity.EntityState == EntityState.Unchanged)
            {
                return;
            }

            var entityEx = entity.As<IEntityStatefulExtension>();

            //判断是否需要更新 FullName
            var updateFullName = metadata.Name != null && metadata.FullName != null &&
                entityEx != null && entityEx.IsModified(metadata.Name.Name);

            var helper = CreatePersisterHelper();
            if (!updateFullName)
            {
                ValidationUnity.Validate(entity);
                helper.Update(entity);
                return;
            }

            var arg = CreateUpdatingArgument(entity);
            var children = GetChildren(arg);
            var fullName = GenerateFullName(entity);
            UpdateChildrenFullName(entity, children, fullName);

            arg.NewValue.FullName = fullName;

            UpdateEntityByArgument(entity, arg);

            ValidationUnity.Validate(entity);

            try
            {
                helper.Batch(() =>
                    {
                        helper.Update(entity);
                        helper.Save(children);
                    });
            }
            catch (Exception ex)
            {
                throw new EntityTreePersistentException(SR.GetString(SRKind.FailInEntityUpdate), ex);
            }
        }

        /// <summary>
        /// 将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        public void Insert(IEntity entity, IEntity referEntity, EntityTreePosition position)
        {
            Guard.ArgumentNull(entity, "entity");

            if (referEntity == null)
            {
                Create(entity);
                return;
            }

            var arg1 = CreateUpdatingArgument(entity);
            var arg2 = CreateUpdatingArgument(referEntity);

            var keyId = arg2.OldValue.InnerId;

            //获得新节点的Order值
            arg1.NewValue.Order = GetNewOrderNumber(arg2.OldValue, position);

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

            IEnumerable brothers = null;
            if (position != EntityTreePosition.Children)
            {
                //获取参考节点的兄弟及其孩子（还有可能包括它自己）
                brothers = GetBrothersAndChildren(arg2, position == EntityTreePosition.Before, null);

                //兄弟及其孩子要下移一个单位
                UpdateBrothersAndChildren(entity, brothers, arg1.NewValue.InnerId, 1);
            }

            UpdateEntityByArgument(entity, arg1);

            ValidationUnity.Validate(entity);

            try 
            {
                var helper = CreatePersisterHelper();
                helper.Batch(() =>
                    {
                        helper.Create(entity);
                        if (brothers != null)
                        {
                            helper.Save(brothers);
                        }
                    });
            }
            catch (Exception ex)
            {
                throw new EntityTreePersistentException(SR.GetString(SRKind.FailInEntityInsert), ex);
            }
        }

        /// <summary>
        /// 将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        public void Move(IEntity entity, IEntity referEntity, EntityTreePosition? position = EntityTreePosition.Children)
        {
            Guard.ArgumentNull(entity, "entity");

            if (referEntity != null && position == null)
            {
                return;
            }

            if (!CheckMovable(entity, referEntity))
            {
                throw new EntityTreePersistentException(SR.GetString(SRKind.FailInEntityMoveWildly), null);
            }

            if (entity.Equals(referEntity) ||
                (position != null && !CheckNeedMove(entity, referEntity, (EntityTreePosition)position)))
            {
                return;
            }

            try
            {
                var arg1 = CreateUpdatingArgument(entity);

                //移到根节点
                if (referEntity == null)
                {
                    UpdateMoveToRoot(entity, arg1);
                    return;
                }

                var arg2 = CreateUpdatingArgument(referEntity);

                if (position == EntityTreePosition.Children)
                {
                    UpdateMoveAsChildren(entity, referEntity, arg1, arg2);
                    return;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                throw new EntityTreePersistentException(SR.GetString(SRKind.FailInEntityMove), ex);
            }
        }

        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对数据假删除的支持。</param>
        public void Remove(IEntity entity, bool fake = true)
        {
            var fakeProperty = PropertyUnity.GetProperties(entityType).FirstOrDefault(s => s.Info.IsDeletedKey);

            try
            {
                var arg = CreateUpdatingArgument(entity);

                //获取它的孩子
                var children = GetChildren(arg);

                //获取它的兄弟及其孩子
                var brothers = GetBrothersAndChildren(arg, false, null);

                var helper = CreatePersisterHelper();
                helper.Batch(() =>
                    {
                        //它的孩子也要移除
                        RemoveChildren(entity, children, fakeProperty != null && fake);

                        //兄弟及其孩子上移一个单位
                        UpdateBrothersAndChildren(entity, brothers, arg.OldValue.InnerId, -1);

                        helper.Save(brothers);
                        helper.Remove(entity, fakeProperty, fake);
                    });
            }
            catch (Exception ex)
            {
                throw new EntityTreePersistentException(SR.GetString(SRKind.FailInEntityMove), ex);
            }
        }

        /// <summary>
        /// 将两个实体的位置进行交换，且相关的子实体也跟随移动。
        /// </summary>
        /// <param name="entityA">要交换的实体A。</param>
        /// <param name="entityB">要交换的实体B。</param>
        public void Swap(IEntity entityA, IEntity entityB)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 将实体在同一层级上进行上移。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        public void ShiftUp(IEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 将实体在同一层级上进行下移。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        public void ShiftDown(IEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 判断两个实体是否具有直属关系。
        /// </summary>
        /// <param name="entityA">实体A。</param>
        /// <param name="entityB">实体B。</param>
        /// <returns></returns>
        public bool IsParental(IEntity entityA, IEntity entityB)
        {
            Guard.ArgumentNull(entityA, "entityA");
            Guard.ArgumentNull(entityB, "entityB");

            var arg1 = ParseEntityData(entityA);
            var arg2 = ParseEntityData(entityB);

            if (arg1.InnerId.Length == arg2.InnerId.Length)
            {
                return false;
            }

            //A是否是B的子节点
            if (arg1.Level > arg2.Level)
            {
                return arg1.InnerId.StartsWith(arg2.InnerId);
            }

            //B是否是A的子节点
            if (arg1.Level < arg2.Level)
            {
                return arg2.InnerId.StartsWith(arg1.InnerId);
            }

            return false;
        }

        /// <summary>
        /// 判断两个实体的父子身份。
        /// </summary>
        /// <param name="entityA">实体A。</param>
        /// <param name="entityB">实体B。</param>
        /// <returns>如果两个实体没有父子关系，则为 0，如果 entityA 是 entityB 的长辈，则为 1，反之为 -1。</returns>
        public int GetPaternalPosition(IEntity entityA, IEntity entityB)
        {
            Guard.ArgumentNull(entityA, "entityA");
            Guard.ArgumentNull(entityB, "entityB");

            var arg1 = ParseEntityData(entityA);
            var arg2 = ParseEntityData(entityB);

            if (arg1.InnerId.Length == arg2.InnerId.Length)
            {
                return 0;
            }

            //A是否是B的子节点
            if (arg1.Level > arg2.Level)
            {
                return arg1.InnerId.StartsWith(arg2.InnerId) ? -1 : 0;
            }

            //B是否是A的子节点
            if (arg1.Level < arg2.Level)
            {
                return arg2.InnerId.StartsWith(arg1.InnerId) ? 1 : 0;
            }

            return 0;
        }

        /// <summary>
        /// 判断两个实体是否具有兄弟关系。
        /// </summary>
        /// <param name="entityA">实体A。</param>
        /// <param name="entityB">实体B。</param>
        /// <returns></returns>
        public bool IsBrotherly(IEntity entityA, IEntity entityB)
        {
            Guard.ArgumentNull(entityA, "entityA");
            Guard.ArgumentNull(entityB, "entityB");

            var bag1 = ParseEntityData(entityA);
            var bag2 = ParseEntityData(entityB);
            return IsBrotherly(bag1, bag2);
        }

        /// <summary>
        /// 获取上一个兄弟。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEntity GetPreviousSibling(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");

            var bag = ParseEntityData(entity);

            var keyId = GetPreviousKey(bag.InnerId);
            var parameters = new ParameterCollection();
            keyId = string.Format("{0}{1}", keyId, new string('_', metadata.SignLength));

            var query = new EntityQueryBuilder(syntax, environment, entityType, parameters)
                .Select().All().From()
                .Where(metadata.InnerSign, keyId, QueryOperator.Like)
                .And(GetOrderExpression() + " < " + bag.Order)
                .OrderBy(metadata.InnerSign);

            return database.InternalExecuteEnumerable(entityType, query.ToSqlCommand(), parameters: parameters).Cast<IEntity>().FirstOrDefault();
        }

        /// <summary>
        /// 获取下一个兄弟。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEntity GetNextSibling(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");

            var bag = ParseEntityData(entity);

            var keyId = GetPreviousKey(bag.InnerId);
            var parameters = new ParameterCollection();
            keyId = string.Format("{0}{1}", keyId, new string('_', metadata.SignLength));

            var query = new EntityQueryBuilder(syntax, environment, entityType, parameters)
                .Select().All().From()
                .Where(metadata.InnerSign, keyId, QueryOperator.Like)
                .And(GetOrderExpression() + " > " + bag.Order)
                .OrderBy(metadata.InnerSign);

            return database.InternalExecuteEnumerable(entityType, query.ToSqlCommand(), parameters: parameters).Cast<IEntity>().FirstOrDefault();
        }

        /// <summary>
        /// 查询指定实体的孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="condition">附加的条件。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <param name="recurrence">是否递归出所有孩子。</param>
        /// <returns></returns>
        public IEnumerable QueryChildren(IEntity entity, string condition = null, ParameterCollection parameters = null, bool recurrence = false)
        {
            var keyId = entity == null ? string.Empty : (string)entity.InternalGetValue(metadata.InnerSign);
            parameters = parameters ?? new ParameterCollection();
            var query = new EntityQueryBuilder(syntax, environment, entityType, parameters).Select().All();
            if (metadata.HasChildren != null)
            {
                query.Single(string.Format("(SELECT (CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END) FROM {0} WHERE {1} LIKE {2} {4}) {3}", 
                    GetTableName(), 
                    QuoteColumn(metadata.InnerSign),
                    syntax.String.Concat("T." + QuoteColumn(metadata.InnerSign), "'" + new string('_', metadata.SignLength) + "'"),
                    QuoteColumn(metadata.HasChildren),
                    !string.IsNullOrEmpty(condition) ? " AND " + condition : string.Empty));
            }

            query = query.From();
            if (recurrence)
            {
                //匹配后面所有
                parameters.Add("pk", keyId + "%");
            }
            else
            {
                //匹配后N位
                parameters.Add("pk", keyId + new string('_', metadata.SignLength));
            }

            query.Where(metadata.InnerSign + " LIKE @pk");
            if (!string.IsNullOrEmpty(condition))
            {
                query.And(condition);
            }

            query.OrderBy(metadata.InnerSign);
            foreach (var item in database.InternalExecuteEnumerable(entityType, query.ToSqlCommand(), parameters: parameters, 
                setter: (r, o) =>
                    {
                        if (metadata.HasChildren != null)
                        {
                            var name = metadata.HasChildren.Name;
                            var rowMap = database.Provider.GetService<IRecordWrapper>();
                            ((IEntity)o).SetValue(metadata.HasChildren.Name, rowMap.GetValue(r, name).To<bool>());
                        }
                    }))
            {
                yield return item;
            }
        }

        /// <summary>
        /// 递归返回实体的父亲实体。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <returns></returns>
        public IEnumerable RecurrenceParent(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");

            var keyId = (string)entity.InternalGetValue(metadata.InnerSign);
            var parameters = new ParameterCollection();
            var query = new EntityQueryBuilder(syntax, environment, entityType, parameters).Select().All().From()
                .Where(metadata.InnerSign, keyId);

            var index = 0;
            var length = keyId.Length / metadata.SignLength;
            while (index < length)
            {
                var newId = GetPreviousKey(keyId, ++index);
                parameters[0].Value = newId;

                foreach (var item in database.InternalExecuteEnumerable(entityType, query.ToSqlCommand(), parameters: parameters))
                {
                    yield return item;
                }
            }
        }
        
        /// <summary>
        /// 判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="condition">附加的条件。</param>
        /// <param name="parameters">查询参数集合。</param>
        /// <returns></returns>
        public bool HasChildren(IEntity entity, string condition = null, ParameterCollection parameters = null)
        {
            var keyId = entity == null ? string.Empty : (string)entity.InternalGetValue(metadata.InnerSign);
            parameters = parameters ?? new ParameterCollection();
            var query = new EntityQueryBuilder(syntax, environment, entityType, parameters).Select().Count().From();
            parameters.Add("pk", keyId + new string('_', metadata.SignLength));
            query.Where(metadata.InnerSign + " LIKE @pk");
            if (!string.IsNullOrEmpty(condition))
            {
                query.And(condition);
            }

            return database.ExecuteScalar<int>(query.ToSqlCommand(), parameters) > 0;
        }

        /// <summary>
        /// 触发 <see cref="EntityTreeUpdating"/> 事件。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="arg"></param>
        /// <param name="action"></param>
        /// <returns>如果客户端已取消，则为 false。</returns>
        private bool RaiseEntityTreeUpdating(IEntity entity, EntityTreeUpfydatingArgument arg, EntityTreeUpdatingAction action)
        {
            var e = new EntityTreeUpdatingEventArgs(entity) { Action = action };
            if (arg != null)
            {
                e.OldValue = arg.OldValue;
                e.NewValue = arg.NewValue;
            }

            return RaiseEntityTreeUpdating(e);
        }

        /// <summary>
        /// 触发 <see cref="EntityTreeUpdating"/> 事件。
        /// </summary>
        /// <param name="e"></param>
        /// <returns>如果客户端已取消，则为 false。</returns>
        private bool RaiseEntityTreeUpdating(EntityTreeUpdatingEventArgs e)
        {
            if (EntityTreeUpdating == null)
            {
                return true;
            }

            EntityTreeUpdating(e);
            return !e.Cancel;
        }

        /// <summary>
        /// 使用 <see cref="EntityTreeUpdatingBag"/> 判断是否具有兄弟关系。
        /// </summary>
        /// <param name="bag1"></param>
        /// <param name="bag2"></param>
        /// <returns></returns>
        private bool IsBrotherly(EntityTreeUpdatingBag bag1, EntityTreeUpdatingBag bag2)
        {
            if ((bag1.InnerId.Length != bag2.InnerId.Length) || bag2.InnerId.Length < metadata.SignLength)
            {
                return false;
            }

            return bag1.InnerId.Substring(0, bag1.InnerId.Length - metadata.SignLength)
                .Equals(bag2.InnerId.Substring(0, bag2.InnerId.Length - metadata.SignLength));
        }

        /// <summary>
        /// 检查必须要有的几个属性。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="metadata"></param>
        private void CheckEntityTreeMetadata(Type entityType, EntityTreeMetadata metadata)
        {
            Guard.Assert(metadata != null, new EntityTreePersistentException(SR.GetString(SRKind.NoneEntityTreeMetadata), null));
            Guard.Assert(metadata.InnerSign != null, new EntityTreeRequiredFieldException(entityType, "InnerSign"));
        }

        /// <summary>
        /// 获取孩子、孙子、重孙...。
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        private IEnumerable GetChildren(EntityTreeUpfydatingArgument argument)
        {
            var parameters = new ParameterCollection();
            var query = new EntityQueryBuilder(syntax, environment, entityType, parameters)
                .Select().Single(GetUseableProperties()).From()
                .Where(metadata.InnerSign, argument.OldValue.InnerId, QueryOperator.UnEquals)
                .And(metadata.InnerSign, argument.OldValue.InnerId + "_%", QueryOperator.Like)
                .OrderBy(metadata.InnerSign);

            return ToList(database.InternalExecuteEnumerable(entityType, query.ToSqlCommand(), parameters: parameters));
        }

        /// <summary>
        /// 获取兄弟及他们的孩子。
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="includeCurrent">是否包含当 <paramref name="argument"/>，当在它前面插入时，需要包含它。</param>
        /// <param name="excludeArg">要排除的实体。</param>
        /// <param name="isTop">是否遇到要排除的实体就终止。</param>
        /// <returns></returns>
        private IEnumerable GetBrothersAndChildren(EntityTreeUpfydatingArgument argument, bool includeCurrent, EntityTreeUpfydatingArgument excludeArg, bool isTop = false)
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
            sb.AppendFormat(" JOIN (SELECT f.{0} {0} FROM {1} f", QuoteColumn(metadata.InnerSign), GetTableName());

            sb.AppendFormat(" WHERE {0} LIKE {1} AND {5} = {6} AND {2} {4} {3}", QuoteColumn(metadata.InnerSign), syntax.FormatParameter("pn"),
                GetOrderExpression(), order, includeCurrent ? ">=" : ">", GetLevelExpression(), level);

            if (m.DeleteProperty != null)
            {
                sb.AppendFormat(" AND {0} = {1}", QuoteColumn(m.DeleteProperty), 0);
            }

            if (excludeArg != null)
            {
                var excludeId = excludeArg.OldValue.InnerId;
                sb.AppendFormat(" AND NOT ({0} LIKE {1})", QuoteColumn(metadata.InnerSign), syntax.FormatParameter("px"));
                parameters.Add("px", excludeId + "%");

                if (isTop)
                {
                    sb.AppendFormat(" AND {0} < {1}", QuoteColumn(metadata.InnerSign), syntax.FormatParameter("px1"));
                    parameters.Add("px1", excludeId);
                }
            }

            if (!includeCurrent)
            {
                sb.AppendFormat(" AND {0} <> {1}", QuoteColumn(metadata.InnerSign), syntax.FormatParameter("pm"));
                parameters.Add("pm", keyId);
            }

            sb.AppendFormat(") f ON t.{0} LIKE {1} ORDER BY {0}", QuoteColumn(metadata.InnerSign), syntax.String.Concat("f." + QuoteColumn(metadata.InnerSign), "'%'"));

            keyId = GetPreviousKey(keyId) + "_%";
            parameters.Add("pn", keyId);

            return ToList(database.InternalExecuteEnumerable(entityType, (SqlCommand)sb.ToString(), parameters: parameters));
        }

        /// <summary>
        /// 更新所有孩子的全名。
        /// </summary>
        /// <param name="current">当前的实体对象。</param>
        /// <param name="children">要更新的子实体对象。</param>
        /// <param name="fuleName">当前实体对象的全名。</param>
        private void UpdateChildrenFullName(IEntity current, IEnumerable children, string fuleName)
        {
            var list = new List<EntityTreeUpdatingBag>();
            foreach (IEntity entity in children)
            {
                var arg = CreateUpdatingArgument(entity);

                var rowInnerId = arg.NewValue.InnerId;

                //取得上一级编码，然后找到父的全名
                var prevRowInnerId = GetPreviousKey(rowInnerId);
                var parentRow = list.FirstOrDefault(s => s.InnerId == prevRowInnerId);
                var newFullName = parentRow == null ? fuleName :
                    parentRow.FullName;

                //父全名+分隔+当前元素的名称
                newFullName = string.Format("{0}{1}{2}", newFullName, metadata.NameSeparator, arg.OldValue.Name);

                arg.NewValue.FullName = newFullName;

                if (!RaiseEntityTreeUpdating(current, arg, EntityTreeUpdatingAction.Rename))
                {
                    continue;
                }

                UpdateEntityByArgument(entity, arg);

                list.Add(arg.NewValue);
            }

            list.Clear();
        }

        private void UpdateChildren(IEntity current, IEnumerable entities, EntityTreeUpfydatingArgument argument)
        {
            foreach (IEntity entity in entities)
            {
                var arg = CreateUpdatingArgument(entity);

                arg.NewValue.Level = argument.NewValue.Level + arg.OldValue.Level - argument.OldValue.Level;
                arg.NewValue.InnerId = argument.NewValue.InnerId + arg.OldValue.InnerId.Substring(argument.OldValue.Level * metadata.SignLength);

                if (metadata.FullName != null && !string.IsNullOrEmpty(argument.NewValue.FullName))
                {
                    arg.NewValue.FullName = argument.NewValue.FullName + metadata.NameSeparator + GetRightFullName(arg.OldValue.FullName, argument.OldValue.Level);
                }

                if (!RaiseEntityTreeUpdating(current, arg, EntityTreeUpdatingAction.Move))
                {
                    continue;
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
        private void UpdateBrothersAndChildren(IEntity current, IEnumerable entities, string currentInnerId, int position)
        {
            var dictionary = new Dictionary<string, EntityTreeUpfydatingArgument>();

            foreach (IEntity entity in entities)
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
                arg.NewValue.InnerId = prevRowInnerId + new string('0', metadata.SignLength - sorder.Length) + sorder;

                if (!RaiseEntityTreeUpdating(current, arg, EntityTreeUpdatingAction.Move))
                {
                    continue;
                }

                UpdateEntityByArgument(entity, arg);
            }
        }

        /// <summary>
        /// 节点移动到根目录下，相关节点的更新。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="arg"></param>
        private void UpdateMoveToRoot(IEntity current, EntityTreeUpfydatingArgument arg)
        {
            //获得新节点的Order值
            var newOrder = GetNewOrderNumber(null, EntityTreePosition.Children);

            //获取它的兄弟及其孩子
            var brothers = GetBrothersAndChildren(arg, false, null);

            //获取它的孩子
            var children = GetChildren(arg);

            //生成新的InnerID
            var currentInnerId = GenerateInnerId(string.Empty, newOrder, EntityTreePosition.Children);

            //全名即为名称
            if (metadata.FullName != null && metadata.Name != null)
            {
                arg.NewValue.FullName = arg.OldValue.Name;
            }

            arg.NewValue.InnerId = currentInnerId;
            arg.NewValue.Level = 1;
            arg.NewValue.Order = newOrder;

            UpdateEntityByArgument(current, arg);

            //兄弟及其孩子要上移一个单位
            UpdateBrothersAndChildren(current, brothers, currentInnerId, -1);

            //它的孩子要移到根节点下
            UpdateChildren(current, children, arg);

            var helper = CreatePersisterHelper();
            helper.Batch(() =>
                {
                    if (RaiseEntityTreeUpdating(current, arg, EntityTreeUpdatingAction.Move))
                    {
                        helper.Update(current);
                    }

                    helper.Save(brothers);
                    helper.Save(children);
                });
        }

        /// <summary>
        /// 将子节点移动到另一个子节点的下面。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="referEntity"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void UpdateMoveAsChildren(IEntity entity, IEntity referEntity, EntityTreeUpfydatingArgument arg1, EntityTreeUpfydatingArgument arg2)
        {
            //获取要移动节点的兄弟及其孩子
            var brothers = GetBrothersAndChildren(arg1, false, null);

            //获取要移动的节点的孩子
            var children = GetChildren(arg1);

            //兄弟及其孩子要下移一个单位
            UpdateBrothersAndChildren(entity, brothers, arg1.OldValue.InnerId, -1);

            var modify = IsInList(referEntity, brothers);
            if (modify != null)
            {
                arg2 = CreateUpdatingArgument(modify);
            }

            var keyId = arg2.OldValue.InnerId;
            //获得新节点的Order值
            arg1.NewValue.Order = GetNewOrderNumber(arg2.OldValue, EntityTreePosition.Children);

            //获得参照节点的级别
            arg1.NewValue.Level = arg2.OldValue.Level + 1;

            //生成新的InnerID
            arg1.NewValue.InnerId = GenerateInnerId(keyId, arg1.NewValue.Order, EntityTreePosition.Children);
            arg1.NewValue.FullName = GenerateFullName(arg1, arg2, EntityTreePosition.Children);

            //更新要移动的节点的孩子
            UpdateChildren(entity, children, arg1);
            UpdateEntityByArgument(entity, arg1);

            var helper = CreatePersisterHelper();
            helper.Batch(() =>
                {
                    if (RaiseEntityTreeUpdating(entity, arg1, EntityTreeUpdatingAction.Move))
                    {
                        helper.Update(entity);
                    }

                    helper.Save(brothers);
                    helper.Save(children);
                });
        }

        /// <summary>
        /// 判断实体是否在指定的集合中。如果它在集合中，它有可能被修改。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEntity IsInList(IEntity entity, IEnumerable entities)
        {
            return entities.Cast<IEntity>().FirstOrDefault(item => item.Equals(entity));
        }

        /// <summary>
        /// 移除所有子节点。
        /// </summary>
        /// <param name="current">当前的实体对象。</param>
        /// <param name="entities">要移除的子实体对象。</param>
        /// <param name="fake">是否假删除。</param>
        private void RemoveChildren(IEntity current, IEnumerable entities, bool fake = true)
        {
            var pkProperties = PropertyUnity.GetPrimaryProperties(entityType).ToList();
            var m = EntityMetadataUnity.GetEntityMetadata(entityType);

            var parameters = new ParameterCollection();
            var sb = new StringBuilder();
            if (m.DeleteProperty != null && fake)
            {
                sb.AppendFormat("UPDATE {0} SET {1} = 1, {2} = '-'", GetTableName(), QuoteColumn(m.DeleteProperty), QuoteColumn(metadata.InnerSign));
            }
            else
            {
                sb.AppendFormat("DELETE FROM {0}", GetTableName());
            }

            var c = 0;

            var sbb = new List<StringBuilder>();
            for (var i = 0; i < pkProperties.Count; i++)
            {
                sbb.Add(new StringBuilder());
            }

            foreach (IEntity entity in entities)
            {
                if(!RaiseEntityTreeUpdating(current, null, EntityTreeUpdatingAction.Remove))
                {
                    continue;
                }

                for (var i = 0; i < pkProperties.Count; i++)
                {
                    var id = entity.InternalGetValue(pkProperties[i]);
                    if (c > 0)
                    {
                        sbb[i].Append(", ");
                    }

                    sbb[i].Append(syntax.FormatParameter("p" + i + "_" + c));
                    parameters.Add(PropertyValueHelper.Set(id, new Parameter("p" + i + "_" + c)));
                }

                c++;
            }

            if (c == 0)
            {
                return;
            }

            sb.Append("WHERE");
            for (var i = 0; i < pkProperties.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" AND ");
                }

                sb.AppendFormat("{0} IN ({1})", QuoteColumn(pkProperties[i]), sbb[i]);
            }

            database.ExecuteNonQuery((SqlCommand)sb.ToString(), parameters);
        }

        /// <summary>
        /// 取参照实体的最大order值。
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="mode"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private int GetNewOrderNumber(EntityTreeUpdatingBag bag, EntityTreePosition mode, int offset = 0)
        {
            if (bag == null)
            {
                return GetNewOrderNumber();
            }

            switch (mode)
            {
                case EntityTreePosition.Children:
                    var sql = string.Format("SELECT MAX({0}) FROM {1} WHERE {2} LIKE {3}",
                        GetOrderExpression(),
                        GetTableName(),
                        QuoteColumn(metadata.InnerSign),
                        syntax.FormatParameter("pm"));
                    var innerId = bag.InnerId;

                    var parameters = new ParameterCollection { { "pm", innerId + new string('_', metadata.SignLength) } };
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
        private int GetNewOrderNumber()
        {
            var sql = string.Format("SELECT MAX({0}) FROM {1} WHERE {2} = {3}",
                GetOrderExpression(),
                GetTableName(),
                syntax.String.Length(QuoteColumn(metadata.InnerSign)),
                metadata.SignLength);

            return database.ExecuteScalar((SqlCommand)sql).To<int>() + 1;
        }

        /// <summary>
        /// 判断是否可以移动。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="referEntity"></param>
        /// <returns></returns>
        private bool CheckMovable(IEntity entity, IEntity referEntity)
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
        private bool CheckNeedMove(IEntity entity, IEntity referEntity, EntityTreePosition position)
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
                bag1.InnerId.Length == bag2.InnerId.Length + metadata.SignLength)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取Order的表达式。
        /// </summary>
        /// <returns></returns>
        private string GetOrderExpression()
        {
            //如果Order没有指定，则取InnerId的后N位转成数字
            if (metadata.Order == null)
            {
                var field = DbUtility.FormatByQuote(syntax, metadata.InnerSign.Info.FieldName);
                return syntax.Convert(
                    syntax.String.Substring(field, syntax.String.Length(field) + " + 1 - " + metadata.SignLength, 
                    metadata.SignLength), DbType.Int32);
            }

            return DbUtility.FormatByQuote(syntax, metadata.Order.Info.FieldName);
        }

        /// <summary>
        /// 获取Level的表达式。
        /// </summary>
        /// <returns></returns>
        private string GetLevelExpression()
        {
            //如果Level没有指定，则取InnerId的长度除以N
            if (metadata.Level == null)
            {
                return syntax.String.Length(DbUtility.FormatByQuote(syntax, metadata.InnerSign.Info.FieldName)) + " / " + metadata.SignLength;
            }

            return DbUtility.FormatByQuote(syntax, metadata.Order.Info.FieldName);
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
            return position == EntityTreePosition.Children || keyId.Length < metadata.SignLength ?
                keyId + new string('0', metadata.SignLength - sOrder.Length) + sOrder :
                GetPreviousKey(keyId) + new string('0', metadata.SignLength - sOrder.Length) + sOrder;
        }

        /// <summary>
        /// 取前面n级内码
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetPreviousKey(string key, int index = 1)
        {
            return key.Length < metadata.SignLength ? key : key.Substring(0, key.Length - (metadata.SignLength * index));
        }

        /// <summary>
        /// 取前面一级的全名
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private string GetPreviousFullName(string fullName)
        {
            var index = fullName.LastIndexOf(metadata.NameSeparator);
            return index != -1 ? fullName.Substring(0, index) : fullName;
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
                var index = s.IndexOf(metadata.NameSeparator);
                if (index == -1)
                {
                    break;
                }

                s = s.Substring(index + 1);
            }

            return s;
        }

        /// <summary>
        /// 使用当前节点生成新的全名
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string GenerateFullName(IEntity entity)
        {
            var fullName = string.Empty;
            if (metadata.Name != null && metadata.FullName != null)
            {
                fullName = (string)entity.InternalGetValue(metadata.FullName);
                var index = fullName.LastIndexOf(metadata.NameSeparator);
                if (index != -1)
                {
                    //取上一级全名
                    fullName = string.Format("{0}{1}{2}", fullName.Substring(0, index), metadata.NameSeparator,
                                             (string)entity.InternalGetValue(metadata.Name));
                }
                else
                {
                    //全名等于名称
                    fullName = (string)entity.InternalGetValue(metadata.Name);
                }
            }

            return fullName;
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
            if (metadata.Name != null && metadata.FullName != null)
            {
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
                    fullName = string.Format("{0}{1}{2}", fullName, metadata.NameSeparator, arg1.NewValue.Name);
                }
            }

            return fullName;
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
            if (!string.IsNullOrEmpty(tableName))
            {
                return tableName;
            }

            if (environment != null)
            {
                tableName = DbUtility.FormatByQuote(syntax, environment.GetVariableTableName(entityType.GetRootType()));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(entityType.GetRootType());
                tableName = DbUtility.FormatByQuote(syntax, metadata.TableName);
            }

            return tableName;
        }

        /// <summary>
        /// 获取可以组织到查询里的属性。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IProperty> GetUseableProperties()
        {
            foreach (var pkProperty in PropertyUnity.GetPrimaryProperties(entityType))
            {
                if (pkProperty != metadata.InnerSign)
                {
                    yield return pkProperty;
                }
            }

            yield return metadata.InnerSign;
            if (metadata.Name != null)
            {
                yield return metadata.Name;
            }

            if (metadata.FullName != null)
            {
                yield return metadata.FullName;
            }

            if (metadata.Order != null)
            {
                yield return metadata.Order;
            }

            if (metadata.Level != null)
            {
                yield return metadata.Level;
            }
        }

        /// <summary>
        /// 根据指定的实体创建一个 <see cref="EntityTreeUpfydatingArgument"/> 对象。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private EntityTreeUpfydatingArgument CreateUpdatingArgument(IEntity entity)
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
        private EntityTreeUpdatingBag ParseEntityData(IEntity entity)
        {
            var data = new EntityTreeUpdatingBag
            {
                InnerId = (string)entity.InternalGetValue(metadata.InnerSign),
            };

            if (metadata.Order != null)
            {
                data.Order = (int)entity.InternalGetValue(metadata.Order);
            }
            else if (!string.IsNullOrEmpty(data.InnerId))
            {
                data.Order = int.Parse(data.InnerId.Right(metadata.SignLength));
            }

            if (metadata.Level != null)
            {
                data.Level = (int)entity.InternalGetValue(metadata.Level);
            }
            else if (!string.IsNullOrEmpty(data.InnerId))
            {
                data.Level = data.InnerId.Length / metadata.SignLength;
            }

            if (metadata.Name != null)
            {
                data.Name = (string)entity.InternalGetValue(metadata.Name);
            }

            if (metadata.FullName != null)
            {
                data.FullName = (string)entity.InternalGetValue(metadata.FullName);
            }

            return data;
        }

        /// <summary>
        /// 使用 <see cref="EntityTreeUpfydatingArgument"/> 更新实体属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="argument"></param>
        /// <param name="force">是否强制修改。</param>
        private void UpdateEntityByArgument(IEntity entity, EntityTreeUpfydatingArgument argument, bool force = false)
        {
            //force强制修改属性
            if (force || argument.OldValue.InnerId != argument.NewValue.InnerId)
            {
                entity.InternalSetValue(metadata.InnerSign, argument.NewValue.InnerId);
            }

            if (metadata.Level != null && 
                (force || argument.OldValue.Level != argument.NewValue.Level))
            {
                entity.InternalSetValue(metadata.Level, argument.NewValue.Level);
            }

            if (metadata.Order != null && 
                (force || argument.OldValue.Order != argument.NewValue.Order))
            {
                entity.InternalSetValue(metadata.Order, argument.NewValue.Order);
            }

            if (metadata.Name != null && 
                (force || argument.OldValue.Name != argument.NewValue.Name))
            {
                entity.InternalSetValue(metadata.Name, argument.NewValue.Name);
            }

            if (metadata.FullName != null && 
                (force || argument.OldValue.FullName != argument.NewValue.FullName))
            {
                entity.InternalSetValue(metadata.FullName, argument.NewValue.FullName);
            }
        }

        /// <summary>
        /// 创建底层的 <see cref="EntityPersisterHelper"/> 对象。
        /// </summary>
        /// <returns></returns>
        private EntityPersisterHelper CreatePersisterHelper()
        {
            return new EntityPersisterHelper(database, entityType, environment);
        }

        /// <summary>
        /// 需要将 <see cref="IEnumerable"/> 转换成 <see cref="IList"/> 表示。
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        private IList ToList(IEnumerable enumerable)
        {
            //创建实体类型的 List<> 泛型
            if (entityListType == null)
            {
                entityListType = typeof(List<>).MakeGenericType(entityType);
            }

            var list = entityListType.New<IList>();
            foreach (var entity in enumerable)
            {
                list.Add(entity);
            }

            return list;
        }

        private class EntityTreeUpfydatingArgument
        {
            public EntityTreeUpdatingBag OldValue { get; set; }

            public EntityTreeUpdatingBag NewValue { get; set; }
        }
    }
}