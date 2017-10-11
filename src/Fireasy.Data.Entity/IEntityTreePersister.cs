// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity
{
    public interface IEntityTreePersister
    {
        void Insert(IEntity entity, IEntity referEntity, EntityTreePosition position);

        void Create(IEntity entity);

        void Move(IEntity entity, IEntity referEntity, EntityTreePosition position);

        IEnumerable RecurrenceParent(IEntity entity);

        IEnumerable QueryChildren(IEntity entity, Expression predicate, bool recurrence);

        bool HasChildren(IEntity entity, Expression predicate);
    }
}
