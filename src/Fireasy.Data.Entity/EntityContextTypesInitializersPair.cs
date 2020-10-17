// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity
{
    internal class EntityContextTypesInitializersPair : Tuple<Dictionary<Type, List<string>>, Action<EntityContext>>
    {
        #region Constructor

        // <summary>
        // Creates a new pair of the given set of entity types and DbSet initializer delegate.
        // </summary>
        public EntityContextTypesInitializersPair(
            Dictionary<Type, List<string>> entityTypeToPropertyNameMap, Action<EntityContext> setsInitializer)
            : base(entityTypeToPropertyNameMap, setsInitializer)
        {
        }

        #endregion

        #region Properties

        // <summary>
        // The entity types part of the pair.
        // </summary>
        public Dictionary<Type, List<string>> EntityTypeToPropertyNameMap
        {
            get { return Item1; }
        }

        // <summary>
        // The EntityRepository properties initializer part of the pair.
        // </summary>
        public Action<EntityContext> SetsInitializer
        {
            get { return Item2; }
        }

        #endregion
    }
}
