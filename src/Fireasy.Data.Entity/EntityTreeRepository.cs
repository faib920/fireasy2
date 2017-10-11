// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Validation;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 提供树实体类型的仓储。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityTreeRepository<TEntity> : IQueryProviderAware where TEntity : class, IEntity
    {
        private EntityRepository<TEntity> repository;
        private EntityMetadata metadata;
        private EntityTreeMetadata metaTree;
        private Type entityType;
        private ISyntaxProvider syntax;
        private IDatabase database;

        /// <summary>
        /// 初始化 <see cref="EntityTreeRepository&lt;TEntity&gt;"/> 类的新实例。
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="context"></param>
        public EntityTreeRepository(EntityRepository<TEntity> repository, InternalContext context)
        {
            this.repository = repository;
            entityType = typeof(TEntity);
            metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            metaTree = metadata.EntityTree;
            database = context.Database;
            syntax = database.Provider.GetService<ISyntaxProvider>();
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
        public virtual void Insert(TEntity entity, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (referEntity == null)
            {
                var arg = CreateUpdatingArgument(entity);

                //获得新节点的Order值
                arg.NewValue.Order = GetNewOrderNumber(null, EntityTreePosition.Children);
                arg.NewValue.Level = 1;

                //生成新的InnerID
                arg.NewValue.FullName = arg.OldValue.Name;
                arg.NewValue.InnerId = GenerateInnerId(string.Empty, arg.NewValue.Order, EntityTreePosition.Children);
                UpdateEntityByArgument(entity, arg);
                repository.Insert(entity);

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

            UpdateEntityByArgument(entity, arg1);

            ValidationUnity.Validate(entity);

            try
            {
                repository.Insert(entity);
                //repository.Batch(brothers, (u, s) => u.Update(s));
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
        public virtual void Move(TEntity entity, TEntity referEntity, EntityTreePosition? position)
        {
            Guard.ArgumentNull(entity, nameof(entity));
            throw new NotImplementedException();
        }

        /// <summary>
        /// 判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public virtual bool HasChildren(TEntity entity, Expression<Func<TEntity, bool>> predicate = null)
        {
            var query = (IQueryable)QueryHelper.CreateQuery<TEntity>(repository.Provider, predicate);
            var mthCount = typeof(Enumerable).GetMethods().FirstOrDefault(s => s.Name == "Count" && s.GetParameters().Length == 2);
            mthCount = mthCount.MakeGenericMethod(typeof(TEntity));

            var expression = TreeExpressionBuilder.BuildHasChildrenExpression(metaTree, entity, predicate);
            expression = Expression.Call(null, mthCount, query.Expression, expression);
            return (int)repository.Provider.Execute(expression) > 0;
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
        /// <returns></returns>
        public IQueryable<TEntity> RecurrenceParent(IEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            var keyId = (string)entity.GetValue(metaTree.InnerSign);

            var parents = new List<string>();
            while ((keyId = keyId.Substring(0, keyId.Length - metaTree.SignLength)).Length > 0)
            {
                parents.Add(keyId);
            }

            var expression = TreeExpressionBuilder.BuildGetByInnerIdExpression<TEntity>(metaTree, parents);

            var querable = QueryHelper.CreateQuery<TEntity>(repository.Provider, expression);

            var orderExp = TreeExpressionBuilder.BuildOrderByLengthDescExpression<TEntity>(metaTree, querable.Expression);

            return repository.Provider.CreateQuery<TEntity>(orderExp);
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
                        DbUtility.FormatByQuote(syntax, metadata.TableName),
                        QuoteColumn(metaTree.InnerSign),
                        syntax.FormatParameter("pm"));
                    var innerId = bag.InnerId;

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
        private int GetNewOrderNumber()
        {
            var sql = string.Format("SELECT MAX({0}) FROM {1} WHERE {2} = {3}",
                GetOrderExpression(),
                DbUtility.FormatByQuote(syntax, metadata.TableName),
                syntax.String.Length(QuoteColumn(metaTree.InnerSign)),
                metaTree.SignLength);

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
        /// 使用当前节点生成新的全名
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string GenerateFullName(IEntity entity)
        {
            var fullName = string.Empty;
            if (metaTree.Name != null && metaTree.FullName != null)
            {
                fullName = (string)entity.GetValue(metaTree.FullName);
                var index = fullName.LastIndexOf(metaTree.NameSeparator);
                if (index != -1)
                {
                    //取上一级全名
                    fullName = string.Format("{0}{1}{2}", fullName.Substring(0, index), metaTree.NameSeparator,
                                             (string)entity.GetValue(metaTree.Name));
                }
                else
                {
                    //全名等于名称
                    fullName = (string)entity.GetValue(metaTree.Name);
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
        /// 获取属性的对应的字段表达式，并在前后加上标识符。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private string QuoteColumn(IProperty property)
        {
            return DbUtility.FormatByQuote(syntax, property.Info.FieldName);
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
            if (metaTree.Name != null)
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
        private void UpdateEntityByArgument(IEntity entity, EntityTreeUpfydatingArgument argument, bool force = false)
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

        private class EntityTreeUpfydatingArgument
        {
            public EntityTreeUpdatingBag OldValue { get; set; }

            public EntityTreeUpdatingBag NewValue { get; set; }
        }
    }
}
