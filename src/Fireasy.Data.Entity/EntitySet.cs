// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 一个实体对象的集合。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    [Serializable]
    public class EntitySet<TEntity> : IEntitySet,
        IList<TEntity>,
        IList,
        ICloneable,
        IEntityRelation
        where TEntity : class, IEntity
    {
        private readonly List<TEntity> _innerList = new List<TEntity>();
        private readonly List<TEntity> _detachedList = new List<TEntity>();
        private bool _isNullset;
        private EntityOwner _owner;
        private EntityOwner _thisOwner;

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.EntitySet`1"/> 类的新实例。
        /// </summary>
        public EntitySet()
        {
            AllowBatchInsert = true;
        }

        /// <summary>
        /// 使用一个 <typeparamref name="TEntity"/> 序列初始化 <see cref="T:Fireasy.Data.Entity.EntitySet`1"/> 类的新实例。
        /// </summary>
        /// <param name="source"></param>
        public EntitySet(IEnumerable<TEntity> source)
            : this()
        {
            foreach (var entity in source)
            {
                SetEntityOwner(entity);
                _innerList.Add(entity);
            }
        }

        /// <summary>
        /// 使用一个 <typeparamref name="TEntity"/> 序列初始化 <see cref="T:Fireasy.Data.Entity.EntitySet`1"/> 类的新实例。
        /// </summary>
        /// <param name="entities"></param>
        public EntitySet(params TEntity[] entities)
            : this()
        {
            foreach (var entity in entities)
            {
                SetEntityOwner(entity);
                _innerList.Add(entity);
            }
        }

        /// <summary>
        /// 使用一个枚举器初始化 <see cref="T:Fireasy.Data.Entity.EntitySet`1"/> 类的新实例。
        /// </summary>
        /// <param name="source"></param>
        public EntitySet(IEnumerable source)
            : this()
        {
            foreach (var item in source)
            {
                SetEntityOwner((TEntity)item);
                _innerList.Add((TEntity)item);
            }
        }

        /// <summary>
        /// 获取或设置是否批量插入集合中的实体。默认为 true。
        /// </summary>
        public bool AllowBatchInsert { get; set; }

        /// <summary>
        /// 获取或设置是否批量更新集合中的实体。默认为 false。
        /// </summary>
        public bool AllowBatchUpdate { get; set; }

        /// <summary>
        /// 添加一个实体对象到集合中。
        /// </summary>
        /// <param name="entity">要添加的实体对象。</param>
        public void Add(TEntity entity)
        {
            entity.SetState(EntityState.Attached);
            SetEntityOwner(entity);
            SetOwnerState();
            _innerList.Add(entity);
        }

        /// <summary>
        /// 移除集合中的所有实体。
        /// </summary>
        public void Clear()
        {
            for (var i = _innerList.Count - 1; i >= 0; i--)
            {
                if (_innerList[i].EntityState != EntityState.Attached)
                {
                    _detachedList.Add(_innerList[i]);
                }
            }

            SetOwnerState();
            _innerList.Clear();
        }

        /// <summary>
        /// 判断实体是否在集合中。
        /// </summary>
        /// <param name="entity">要查找的实体对象。</param>
        /// <returns></returns>
        public bool Contains(TEntity entity)
        {
            return _innerList.Contains(entity);
        }

        /// <summary>
        /// 将集合中的所有实体复制到数组中。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 从集合中移除指定的实体对象。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <returns></returns>
        public bool Remove(TEntity entity)
        {
            if (entity.EntityState != EntityState.Attached)
            {
                _detachedList.Add(entity);
            }

            SetOwnerState();
            return _innerList.Remove(entity);
        }

        /// <summary>
        /// 获取集合中实体的个数。
        /// </summary>
        public int Count
        {
            get { return _innerList.Count; }
        }

        /// <summary>
        /// 获取集合是否只读。
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// 返回指定实体在集合中从零开始的索引位置。
        /// </summary>
        /// <param name="entity">要查找的实体对象。</param>
        /// <returns></returns>
        public int IndexOf(TEntity entity)
        {
            return _innerList.IndexOf(entity);
        }

        /// <summary>
        /// 将实体对象插入到集合中的指定位置。
        /// </summary>
        /// <param name="index">插入的位置。</param>
        /// <param name="entity">要插入的实体对象。</param>
        public void Insert(int index, TEntity entity)
        {
            Guard.OutOfRange(Count, index);
            entity.SetState(EntityState.Attached);
            SetEntityOwner(entity);
            SetOwnerState();
            _innerList.Insert(index, entity);
        }

        /// <summary>
        /// 移除指定位置的实体对象。
        /// </summary>
        /// <param name="index">要移除的位置。</param>
        public void RemoveAt(int index)
        {
            Guard.OutOfRange(Count, index);
            if (_innerList[index].EntityState != EntityState.Attached)
            {
                _detachedList.Add(_innerList[index]);
            }

            SetOwnerState();
            _innerList.RemoveAt(index);
        }

        /// <summary>
        /// 获取或设置指定索引处的实体对象。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TEntity this[int index]
        {
            get
            {
                Guard.OutOfRange(Count, index);
                return _innerList[index];
            }
            set
            {
                Guard.OutOfRange(Count, index);
                _innerList[index] = value;
                _innerList[index].SetState(EntityState.Modified);
            }
        }

        /// <summary>
        /// 返回循环返回实体的枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        #region 实现IList

        int IList.Add(object value)
        {
            Add((TEntity)value);
            return Count;
        }

        bool IList.Contains(object value)
        {
            return Contains((TEntity)value);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (TEntity)value; }
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((TEntity)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (TEntity)value);
        }

        void IList.Remove(object value)
        {
            Remove((TEntity)value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((TEntity[])array, index);
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }
        #endregion

        #region 实现IEntitySetInternalExtension接口

        Type IEntitySet.EntityType
        {
            get { return typeof(TEntity); }
        }

        IEnumerable<IEntity> IEntitySet.GetDetachedList()
        {
            return _detachedList;
        }

        IEnumerable<IEntity> IEntitySet.GetAttachedList()
        {
            return _innerList.Where(s => s.EntityState == EntityState.Attached).ToList();
        }

        IEnumerable<IEntity> IEntitySet.GetModifiedList()
        {
            return _innerList.Where(s => s.EntityState == EntityState.Modified).ToList();
        }

        void IEntitySet.Reset()
        {
            _detachedList.Clear();
        }

        bool IEntitySet.IsNeedClear
        {
            get { return _isNullset; }
            set
            {
                _isNullset = value;
                if (value)
                {
                    Clear();
                }
            }
        }

        /// <summary>
        /// 转移移除的列表到当前集合中。
        /// </summary>
        /// <param name="source"></param>
        void IEntitySet.ShiftDetachedList(IEntitySet source)
        {
            foreach (var item in source.GetDetachedList())
            {
                _detachedList.Add((TEntity)item);
            }
        }

        #endregion

        private void SetOwnerState()
        {
            if (_owner != null)
            {
                _owner.Parent.As<IEntityRelation>(s => s.NotifyModified(_owner.Property.Name));
            }
        }

        private void SetEntityOwner(IEntity entity)
        {
            if (_thisOwner == null)
            {
                _thisOwner = new EntityOwner(this, null);
            }

            entity.As<IEntityRelation>(s => s.Owner = _thisOwner);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 获取或设置集合所属的实体对象。
        /// </summary>
        EntityOwner IEntityRelation.Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        void IEntityRelation.NotifyModified(string propertyName)
        {
            if (_owner != null)
            {
                _owner.Parent.As<IEntityRelation>(s => s.NotifyModified(_owner.Property == null ? string.Empty : _owner.Property.Name));
            }
        }

        /// <summary>
        /// 克隆一个对象。
        /// </summary>
        /// <returns>新的对象。</returns>
        public virtual object Clone()
        {
            var list = new EntitySet<TEntity>();
            foreach (var item in _innerList)
            {
                item.As<ICloneable>(s => list.Add((TEntity)s.Clone()));
            }

            return list;
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
