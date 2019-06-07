// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 表示数据实体。该类型通过定义多个静态的 <see cref="IProperty"/> 来映射业务实体的属性。
    /// </summary>
    [Serializable]
    public abstract class EntityObject :
        IEntity,
        ICloneable,
        IKeepStateCloneable,
        INotifyPropertyChanging,
        INotifyPropertyChanged,
        IEntityPersistentEnvironment,
        IEntityPersistentInstanceContainer,
        ISupportInitializeNotification,
        ILazyManager,
        IEntityRelation,
        IPropertyFieldMappingResolver
    {
        private EntityEntryDictionary valueEntry;
        private EntityOwner owner;
        [NonSerialized]
        private bool isModifing;
        [NonSerialized]
        private EntityPersistentEnvironment environment;
        [NonSerialized]
        private EntityLzayManager lazyMgr;

        /// <summary>
        /// 在属性即将修改时，通知客户端应用程序。
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// 在属性修改之后，通知客户端应用程序。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 实体初始化完成的事件。
        /// </summary>
        public event EventHandler Initialized;

        private Type entityType;
        private EntityState state;
        private bool isInitialized;

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.EntityObject"/> 类的新实例。对象的初始状态为 Attached。
        /// </summary>
        protected EntityObject()
        {
            entityType = GetType();
            state = EntityState.Attached;
        }

        private EntityLzayManager InnerLazyMgr
        {
            get
            {
                return lazyMgr ?? (lazyMgr = new EntityLzayManager(entityType));
            }
        }

        private EntityEntryDictionary InnerEntry
        {
            get
            {
                return valueEntry ?? (valueEntry = new EntityEntryDictionary());
            }
        }

        /// <summary>
        /// 获取指定属性的值。
        /// </summary>
        /// <param name="property">实体属性。</param>
        /// <returns></returns>
        public virtual PropertyValue GetValue(IProperty property)
        {
            if (property == null)
            {
                return PropertyValue.Empty;
            }

            var hasValue = InnerEntry.Has(property.Name);
            var value = PropertyValue.Empty;
            if (hasValue)
            {
                value = InnerEntry[property.Name].GetCurrentValue();
            }
            else if (property.Type.IsValueType)
            {
                value = PropertyValue.NewValue(property.Type.GetDefaultValue(), property.Type);
            }

            //关联属性
            if (!hasValue && property is RelationProperty)
            {
                value = ProcessSupposedProperty(property);
            }

            return EntityUtility.CheckReturnValue(property, value);
        }

        /// <summary>
        /// 设置指定属性的值。
        /// </summary>
        /// <param name="property">实体属性。</param>
        /// <param name="value">要设置的值。</param>
        public virtual void SetValue(IProperty property, PropertyValue value)
        {
            PropertyValue oldValue;
            //如果赋值相同则忽略更改
            if (CheckValueEquals(property, value, out oldValue))
            {
                return;
            }

            //验证属性值
            //ValidationUnity.Validate(this, property, value);
            EntityUtility.CheckPrimaryProperty(this, property);

            if (CheckPropertyChangingIsCanceled(property, value, oldValue))
            {
                return;
            }

            CheckEntitySetDestroy(property, oldValue, value);
            InternalSetValue(property, value);
            EntityUtility.UpdateFromReference(property, this, value);
            this.As<IEntityRelation>(s => s.NotifyModified(property.Name));

            OnPropertyChanged(new PropertyChangedEventArgs(property, oldValue, value));
        }

        /// <summary>
        /// 初始化属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public virtual void InitializeValue(IProperty property, PropertyValue value)
        {
            if (property is RelationProperty)
            {
                InnerLazyMgr.SetValueCreated(property.Name);
            }

            InnerEntry.Initializate(property.Name, value);
        }

        /// <summary>
        /// 触发属性即将修改的通知事件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            PropertyChanging?.Invoke(this, e);
        }

        /// <summary>
        /// 触发对象初始化的通知事件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnInitialized(EventArgs e)
        {
            Initialized?.Invoke(this, e);
        }

        /// <summary>
        /// 触发属性已被修改的通知事件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        #region 实现IEntity
        /// <summary>
        /// 获取指定属性的值。
        /// </summary>
        /// <param name="propertyName">实体属性。</param>
        /// <returns></returns>
        PropertyValue IEntity.GetValue(string propertyName)
        {
            var property = PropertyUnity.GetProperty(entityType, propertyName);
            if (property == null)
            {
                throw new PropertyNotFoundException(propertyName);
            }

            return GetValue(property);
        }

        /// <summary>
        /// 设置指定属性的值。
        /// </summary>
        /// <param name="propertyName">实体属性。</param>
        /// <param name="value">要设置的值。</param>
        void IEntity.SetValue(string propertyName, PropertyValue value)
        {
            var property = PropertyUnity.GetProperty(entityType, propertyName);
            if (property == null)
            {
                throw new PropertyNotFoundException(propertyName);
            }

            SetValue(property, value);
        }

        /// <summary>
        /// 获取或设置持久化环境。
        /// </summary>
        EntityPersistentEnvironment IEntityPersistentEnvironment.Environment
        {
            get { return environment; }
            set { environment = value; }
        }

        string IEntityPersistentInstanceContainer.InstanceName { get; set; }

        /// <summary>
        /// 获取实体的状态。
        /// </summary>
        /// <remarks>
        /// <para>使用构造函数创建，或往集合中添加的对象为 Attached；</para>
        /// <para>持久化后的对象为 Unchanged；</para>
        /// <para>一旦属性被修改为 Modified；</para>
        /// <para>从集合内移除时为 Detached。</para>
        /// <para>只有状态不为 Unchanged 的实体才能够持久化。</para>
        /// </remarks>
        EntityState IEntity.EntityState
        {
            get { return state; }
        }

        Type IEntity.EntityType
        {
            get { return entityType; }
        }

        void IEntity.SetState(EntityState state)
        {
            this.state = state;
        }

        void IEntity.ResetUnchanged()
        {
            state = EntityState.Unchanged;
            InnerEntry.Reset();
        }

        string[] IEntity.GetModifiedProperties()
        {
            return (from s in InnerEntry.GetModifiedProperties()
                    let p = PropertyUnity.GetProperty(entityType, s)
                    where p != null && (!p.Info.IsPrimaryKey || (p.Info.IsPrimaryKey && p.Info.GenerateType == IdentityGenerateType.None))
                    select s).ToArray();
        }

        PropertyValue IEntity.GetOldValue(IProperty property)
        {
            return InnerEntry.Has(property.Name) ? InnerEntry[property.Name].GetOldValue() : PropertyValue.Empty;
        }

        PropertyValue IEntity.GetDirectValue(IProperty property)
        {
            return InnerEntry.Has(property.Name) ? InnerEntry[property.Name].GetCurrentValue() : PropertyValue.Empty;
        }

        bool IEntity.IsModifyLocked
        {
            get { return isModifing; }
            set { isModifing = value; }
        }

        void IEntity.NotifyModified(string propertyName, bool modified)
        {
            if (!modified)
            {
                InnerEntry.Modify(propertyName, false);
                return;
            }

            InnerEntry.Modify(propertyName, modified);

            if (state == EntityState.Unchanged)
            {
                state = EntityState.Modified;
            }

            if (owner != null)
            {
                owner.Parent.As<IEntity>(s => s.NotifyModified(owner.Property == null ? string.Empty : owner.Property.Name));
            }
        }

        bool IEntity.IsModified(string propertyName)
        {
            return InnerEntry.Has(propertyName) && InnerEntry[propertyName].IsModified;
        }
        #endregion

        #region 实现IEntityRelation
        EntityOwner IEntityRelation.Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        void IEntityRelation.NotifyModified(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                InnerEntry.Modify(propertyName);
            }

            if (state == EntityState.Unchanged)
            {
                state = EntityState.Modified;
            }

            //修改父对象的状态
            if (owner != null)
            {
                owner.Parent.As<IEntityRelation>(s => s.NotifyModified(owner.Property == null ? string.Empty : owner.Property.Name));
            }
        }
        #endregion

        #region 实现ISupportInitializeNotification
        /// <summary>
        /// 初始化实体数据开始。
        /// </summary>
        public virtual void BeginInit()
        {
            isInitialized = false;
        }

        /// <summary>
        /// 初始化实体数据结束。
        /// </summary>
        public virtual void EndInit()
        {
            isInitialized = true;
            OnInitialized(new EventArgs());
        }

        /// <summary>
        /// 获取实体数据是否已经初始化。
        /// </summary>
        bool ISupportInitializeNotification.IsInitialized
        {
            get { return isInitialized; }
        }
        #endregion

        #region 实现 ILazyManager
        /// <summary>
        /// 判断属性是否已经创建。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <returns></returns>
        bool ILazyManager.IsValueCreated(string propertyName)
        {
            return InnerLazyMgr.IsValueCreated(propertyName);
        }
        #endregion

        #region Object
        /// <summary>
        /// 判断当前对象是否与指定的对象相等。
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public override bool Equals(object right)
        {
            if (ReferenceEquals(right, null))
            {
                return false;
            }

            if (ReferenceEquals(this, right))
            {
                return true;
            }

            if (GetType() != right.GetType())
            {
                return false;
            }

            return GetHashCode() == right.GetHashCode();
        }

        /// <summary>
        /// 返回当前对象的哈希值。
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
        {
            var pkProperties = PropertyUnity.GetPrimaryProperties(entityType);
            return pkProperties.Select(GetValue).Aggregate(0, (current, value) => current ^ (value == null ? 0 : value.GetHashCode()));
        }

        /// <summary>
        /// 克隆出一个新的实体对象。克隆的新实体状态为 <see cref="EntityState.Attached"/>，且所有的属性变为被修改的。
        /// </summary>
        /// <returns></returns>
	    public object Clone()
        {
            return CloneInternal(false, false);
        }

        /// <summary>
        /// 克隆出一个新的实体对象。
        /// </summary>
        /// <param name="dismodified">如果为 true，将丢弃实体被修改后的属性值，沿用原来的值。</param>
        /// <returns></returns>
        public IEntity Clone(bool dismodified = false)
        {
            return (IEntity)CloneInternal(false, !dismodified);
        }

        object IKeepStateCloneable.Clone()
        {
            return CloneInternal(true, false);
        }

        /// <summary>
        /// 克隆实体。
        /// </summary>
        /// <param name="keepState">新实例是事保持原来实体的状态。</param>
        /// <param name="readOldValue"></param>
        /// <returns></returns>
        private object CloneInternal(bool keepState, bool readOldValue)
        {
            var entity = entityType.New<EntityObject>();
            if (keepState)
            {
                entity.SetState(state);
            }

            entity.InitializeInstanceName((this.As<IEntityPersistentInstanceContainer>().InstanceName));
            entity.InitializeEnvironment(environment);
            this.TryLockModifing(() =>
                {
                    foreach (var k in InnerEntry)
                    {
                        //保持状态且值没有修改
                        if (keepState && !k.Value.IsModified)
                        {
                            entity.InnerEntry.Initializate(k.Key, k.Value.GetOldValue().Clone());
                        }
                        else
                        {
                            entity.InnerEntry.Modify(k.Key, readOldValue
                                ? k.Value.GetOldValue().Clone() :
                                k.Value.GetCurrentValue().Clone());
                        }
                    }
                });

            return entity;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 处理附加属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private PropertyValue ProcessSupposedProperty(IProperty property)
        {
            var relationProperty = property.As<RelationProperty>();
            if (relationProperty != null &&
                relationProperty.Options.LoadBehavior != LoadBehavior.None)
            {
                var value = EntityLazyloader.Load(this, relationProperty);
                InnerLazyMgr.SetValueCreated(property.Name);

                if (value != null)
                {
                    InnerEntry.Initializate(property.Name, value, () => value.DataType = property.Info.DataType);
                }

                return value;
            }

            return PropertyValue.Empty;
        }

        /// <summary>
        /// 检查是否有原值相等。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        private bool CheckValueEquals(IProperty property, PropertyValue value, out PropertyValue oldValue)
        {
            if (InnerEntry.Has(property.Name) &&
                (oldValue = (this as IEntity).GetDirectValue(property)) != null)
            {
                oldValue.InitializeInstanceName(string.Empty);
                return value == oldValue;
            }

            oldValue = PropertyValue.Empty;

            return false;
        }

        /// <summary>
        /// 检查客户端是否取消属性值的更改。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        private bool CheckPropertyChangingIsCanceled(IProperty property, PropertyValue value, PropertyValue oldValue)
        {
            var changingArgs = new PropertyChangingEventArgs(property, oldValue, value);
            OnPropertyChanging(changingArgs);
            return changingArgs.Cancel;
        }

        /// <summary>
        /// 检查 EntitySet 里的元素是还被销毁。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        private bool CheckEntitySetDestroy(IProperty property, PropertyValue oldValue, PropertyValue newValue)
        {
            if (!property.Is<EntitySetProperty>())
            {
                return false;
            }

            if (PropertyValue.IsEmpty(oldValue))
            {
                oldValue = ProcessSupposedProperty(property);
            }

            if (!PropertyValue.IsEmpty(oldValue) && PropertyValue.IsEmpty(newValue))
            {
                var oldSet = oldValue.GetValue() as IEntitySet;
                if (oldSet != null)
                {
                    oldSet.Clear();
                }

                if (!PropertyValue.IsEmpty(newValue))
                {
                    var newSet = newValue.GetValue() as IEntitySet;
                    if (newSet != null)
                    {
                        newSet.ShiftDetachedList(oldSet);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 内部设置属性值的方法。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void InternalSetValue(IProperty property, PropertyValue value)
        {
            if (PropertyValue.IsEmpty(value))
            {
                if (SetNullable(property))
                {
                    return;
                }
            }

            if (property is RelationProperty)
            {
                InnerLazyMgr.SetValueCreated(property.Name);
            }

            InnerEntry.Modify(property.Name, value);
        }

        /// <summary>
        /// 将关联属性的值设为 null。
        /// </summary>
        /// <param name="property"></param>
        private bool SetNullable(IProperty property)
        {
            var relationPro = property.As<RelationProperty>();
            if (relationPro == null)
            {
                return false;
            }

            var oldValue = CheckAndLazyPropertyValue(property);
            if (oldValue == null)
            {
                return false;
            }

            switch (relationPro.RelationalPropertyType)
            {
                case RelationPropertyType.Entity:
                    EntityUtility.SetEntityToNull(oldValue);
                    break;
                case RelationPropertyType.EntitySet:
                    EntityUtility.SetEntitySetToNull(oldValue);
                    break;
            }

            return true;
        }

        /// <summary>
        /// 检查并懒加载属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private PropertyValue CheckAndLazyPropertyValue(IProperty property)
        {
            var value = PropertyValue.Empty;
            if (InnerEntry.Has(property.Name))
            {
                value = InnerEntry[property.Name].GetOldValue();
            }

            return PropertyValue.IsEmpty(value) ? ProcessSupposedProperty(property) : value;
        }
        #endregion

        #region 实现IPropertyFieldMappingResolver
        IEnumerable<PropertyFieldMapping> IPropertyFieldMappingResolver.GetDbMapping()
        {
            return EntityPropertyFieldMappingResolver.GetDbMapping(entityType);
        }
        #endregion
    }
}
