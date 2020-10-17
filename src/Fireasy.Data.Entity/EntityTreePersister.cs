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
using System.Linq.Expressions;
using System.Reflection;
using Fireasy.Data.Entity.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Common;
using Fireasy.Common.Extensions;
using System.Collections;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 为实体树提供一组特殊的数据持久化方法。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public class EntityTreePersister<TEntity> : EntityPersister<TEntity>, IEntityTreePersister where TEntity : class, IEntity
    {
        private EntityTreeMetadata metadata;
        private static PropertyInfo StringLengthProperty = typeof(string).GetProperty("Length");
        private static MethodInfo StringStartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });

        /// <summary>
        /// 实体树更新时通知客户端程序。
        /// </summary>
        public event EntityTreeUpdatingEventHandler<TEntity> EntityTreeUpdating;

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.EntityTreePersister`1"/> 类的新实例。
        /// </summary>
        /// <param name="instanceName">实例名。</param>
        public EntityTreePersister(string instanceName = null)
            : base (instanceName)
        {
        }

        /// <summary>
        /// 使用 <see cref="IDatabase"/> 对象初始化 <see cref="T:Fireasy.Data.Entity.EntityTreePersister`1"/> 类的新实例。
        /// </summary>
        /// <param name="database"></param>
        public EntityTreePersister(IDatabase database)
            : base(database)
        {
        }
        /// <summary>
        /// 使用 <see cref="InternalContext"/> 对象初始化 <see cref="T:Lord.Data.Entity.EntityTreePersister`1"/> 类的新实例。
        /// </summary>
        /// <param name="context"></param>
        public EntityTreePersister(InternalContext context)
            : base(context)
        {
        }

        /// <summary>
        /// 将一个新的 <typeparamref name="TEntity"/> 对象持久化。
        /// </summary>
        /// <param name="entity">要持久化的实体对象。</param>
        public override void Create(TEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");

            var helper = CreatePersisterHelper();
            helper.Create(entity);
        }

        /// <summary>
        /// 更新实体对象的修改。如果已经定义了 FullName 则会更新当前对象及相关的子实体对象 的 FullName 属性。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        public override void Update(TEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");

            var helper = CreatePersisterHelper();
            helper.Update(entity);
        }

        /// <summary>
        /// 将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        public virtual void Insert(TEntity entity, TEntity referEntity, EntityTreePosition position)
        {
            Guard.ArgumentNull(entity, "entity");

            var helper = CreatePersisterHelper();
            helper.Insert(entity, referEntity, position);
        }

        /// <summary>
        /// 将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        public virtual void Move(TEntity entity, TEntity referEntity, EntityTreePosition? position)
        {
            Guard.ArgumentNull(entity, "entity");

            var helper = CreatePersisterHelper();
            helper.Move(entity, referEntity, position);
        }

        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对数据假删除的支持。</param>
        public override void Remove(TEntity entity, bool fake = true)
        {
            Guard.ArgumentNull(entity, "entity");

            var helper = CreatePersisterHelper();
            helper.Remove(entity, fake);
        }

        /// <summary>
        /// 将两个实体的位置进行交换，且相关的子实体也跟随移动。
        /// </summary>
        /// <param name="entityA">要交换的实体A。</param>
        /// <param name="entityB">要交换的实体B。</param>
        public virtual void Swap(TEntity entityA, TEntity entityB)
        {
            var helper = CreatePersisterHelper();
            helper.Swap(entityA, entityB);
        }

        /// <summary>
        /// 将实体在同一层级上进行上移。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        public virtual void ShiftUp(TEntity entity)
        {
            var helper = CreatePersisterHelper();
            helper.ShiftUp(entity);
        }

        /// <summary>
        /// 将实体在同一层级上进行下移。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        public virtual void ShiftDown(TEntity entity)
        {
            var helper = CreatePersisterHelper();
            helper.ShiftDown(entity);
        }

        /// <summary>
        /// 判断两个实体是否具有直属关系。
        /// </summary>
        /// <param name="entityA">实体A。</param>
        /// <param name="entityB">实体B。</param>
        /// <returns></returns>
        public virtual bool IsParental(TEntity entityA, TEntity entityB)
        {
            var helper = CreatePersisterHelper();
            return helper.IsParental(entityA, entityB);
        }

        /// <summary>
        /// 判断两个实体的父子身份。
        /// </summary>
        /// <param name="entityA">实体A。</param>
        /// <param name="entityB">实体B。</param>
        /// <returns>如果两个实体没有父子关系，则为 0，如果 entityA 是 entityB 的长辈，则为 1，反之为 -1。</returns>
        public virtual int GetPaternalPosition(TEntity entityA, TEntity entityB)
        {
            var helper = CreatePersisterHelper();
            return helper.GetPaternalPosition(entityA, entityB);
        }

        /// <summary>
        /// 判断两个实体是否具有兄弟关系。
        /// </summary>
        /// <param name="entityA">实体A。</param>
        /// <param name="entityB">实体B。</param>
        /// <returns></returns>
        public virtual bool IsBrotherly(TEntity entityA, TEntity entityB)
        {
            var helper = CreatePersisterHelper();
            return helper.IsBrotherly(entityA, entityB);
        }

        /// <summary>
        /// 判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public virtual bool HasChildren(TEntity entity, Expression<Func<TEntity, bool>> predicate = null)
        {
            var query = (IQueryable)QueryHelper.CreateQuery<TEntity>(Provider, predicate);
            var mthCount = typeof(Enumerable).GetMethods().FirstOrDefault(s => s.Name == "Count" && s.GetParameters().Length == 2);
            mthCount = mthCount.MakeGenericMethod(typeof(TEntity));

            var expression = TreeExpressionBuilder.BuildHasChildrenExpression<TEntity>(GetMetadata(), entity, predicate);
            expression = Expression.Call(null, mthCount, query.Expression, expression);
            return (int)Provider.Execute(expression) > 0;
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
            var expression = TreeExpressionBuilder.BuildQueryChildrenExpression<TEntity>(GetMetadata(), entity, predicate, recurrence);
            return QueryHelper.CreateQuery<TEntity>(Provider, expression);
        }

        /// <summary>
        /// 递归返回实体的父亲实体。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> RecurrenceParent(TEntity entity)
        {
            var helper = CreatePersisterHelper();
            return helper.RecurrenceParent(entity).Cast<TEntity>();
        }

        /// <summary>
        /// 获取上一个兄弟。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual TEntity GetPreviousSibling(TEntity entity)
        {
            var helper = CreatePersisterHelper();
            return helper.GetPreviousSibling(entity) as TEntity;
        }

        /// <summary>
        /// 获取下一个兄弟。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual TEntity GetNextSibling(TEntity entity)
        {
            var helper = CreatePersisterHelper();
            return helper.GetNextSibling(entity) as TEntity;
        }

        protected virtual void OnEntityTreeUpdating(EntityTreeUpdatingEventArgs<TEntity> e)
        {
            if (EntityTreeUpdating != null)
            {
                EntityTreeUpdating(this, e);
            }
        }

        private EntityTreeMetadata GetMetadata()
        {
            return metadata ?? (metadata = EntityMetadataUnity.GetEntityMetadata(typeof (TEntity)).EntityTree);
        }

        private EntityTreePersistentHelper CreatePersisterHelper()
        {
            var helper = new EntityTreePersistentHelper(Database, GetEntityType(), GetMetadata(), Environment);
            helper.EntityTreeUpdating += e =>
                {
                    var ee = new EntityTreeUpdatingEventArgs<TEntity>(e);
                    OnEntityTreeUpdating(ee);
                    e.Cancel = ee.Cancel;
                };

            return helper;
        }

        #region IEntityTreePersister
        void IEntityTreePersister.Insert(IEntity entity, IEntity referEntity, EntityTreePosition position)
        {
            Insert((TEntity)entity, (TEntity)referEntity, position);
        }

        void IEntityTreePersister.Create(IEntity entity)
        {
            Create((TEntity)entity);
        }

        void IEntityTreePersister.Move(IEntity entity, IEntity referEntity, EntityTreePosition position)
        {
            Move((TEntity)entity, (TEntity)referEntity, position);
        }

        IEnumerable IEntityTreePersister.RecurrenceParent(IEntity entity)
        {
            return RecurrenceParent((TEntity)entity);
        }

        IEnumerable IEntityTreePersister.QueryChildren(IEntity entity, Expression predicate, bool recurrence)
        {
            return QueryChildren((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate, recurrence);
        }

        bool IEntityTreePersister.HasChildren(IEntity entity, Expression predicate)
        {
            return HasChildren((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate);
        }
        #endregion
    }

    internal class TreeExpressionBuilder
    {
        private static MethodInfo MthLike = typeof(StringExtension).GetMethod("Like", BindingFlags.Public | BindingFlags.Static);

        internal static Expression BuildQueryChildrenExpression<T>(EntityTreeMetadata metadata, T parent, Expression<Func<T, bool>> predicate, bool recurrence = false) where T : class, IEntity
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
            var no = parent == null ? string.Empty : (string)parent.GetValue(metadata.InnerSign.Name);

            Expression condition = null;
            if (recurrence)
            {
                condition = Expression.Call(null, MthLike, memberExp, Expression.Constant(string.Concat(no, "%")));
            }
            else
            {
                condition = Expression.Call(null, MthLike, memberExp, Expression.Constant(string.Concat(no, new string('_', metadata.SignLength))));
            }

            if (predicate != null)
            {
                var lambda = GetLambda(predicate);
                if (lambda != null)
                {
                    condition = condition.And(DbExpressionReplacer.Replace(lambda.Body, lambda.Parameters[0], parExp));
                }
            }

            return Expression.Lambda<Func<T, bool>>(condition, parExp);
        }

        internal static Expression BuildHasChildrenExpression<T>(EntityTreeMetadata metadata, T parent, Expression<Func<T, bool>> predicate) where T : class, IEntity
        {
            var parExp = Expression.Parameter(typeof(T), "s");

            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
            var no = parent == null ? string.Empty : (string)parent.GetValue(metadata.InnerSign.Name);
            var condition = (Expression)Expression.Call(null, MthLike, memberExp, Expression.Constant(string.Concat(no, new string('_', metadata.SignLength))));

            if (predicate != null)
            {
                var lambda = GetLambda(predicate);
                if (lambda != null)
                {
                    condition = condition.And(DbExpressionReplacer.Replace(lambda.Body, lambda.Parameters[0], parExp));
                }
            }

            return Expression.Lambda<Func<T, bool>>(condition, parExp);
        }

        private static LambdaExpression GetLambda(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            if (e.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)e).Value as LambdaExpression;
            }
            return e as LambdaExpression;
        }

    }
}
