using System;

namespace Fireasy.Data.Entity
{
    internal abstract class VirEntity : IEntity
    {
        Type IEntity.EntityType => throw new NotImplementedException();

        EntityState IEntity.EntityState => throw new NotImplementedException();

        bool IEntity.IsModifyLocked { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IEntity IEntity.Clone(bool dismodified)
        {
            throw new NotImplementedException();
        }

        PropertyValue IEntity.GetDirectValue(IProperty property)
        {
            throw new NotImplementedException();
        }

        string[] IEntity.GetModifiedProperties()
        {
            throw new NotImplementedException();
        }

        PropertyValue IEntity.GetOldValue(IProperty property)
        {
            throw new NotImplementedException();
        }

        PropertyValue IEntity.GetValue(string propertyName)
        {
            throw new NotImplementedException();
        }

        PropertyValue IEntity.GetValue(IProperty property)
        {
            throw new NotImplementedException();
        }

        void IEntity.InitializeValue(IProperty property, PropertyValue value)
        {
            throw new NotImplementedException();
        }

        bool IEntity.IsModified(string propertyName)
        {
            throw new NotImplementedException();
        }

        void IEntity.NotifyModified(string propertyName)
        {
            throw new NotImplementedException();
        }

        void IEntity.ResetUnchanged()
        {
            throw new NotImplementedException();
        }

        void IEntity.SetState(EntityState state)
        {
            throw new NotImplementedException();
        }

        void IEntity.SetValue(string propertyName, PropertyValue value)
        {
            throw new NotImplementedException();
        }

        void IEntity.SetValue(IProperty property, PropertyValue value)
        {
            throw new NotImplementedException();
        }
    }
}
