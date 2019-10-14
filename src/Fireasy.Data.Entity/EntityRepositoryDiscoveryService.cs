// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Initializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    internal class EntityRepositoryDiscoveryService
    {
        #region Fields and constructors

        // AppDomain cache collection initializers for a known type.
        private static readonly SafetyDictionary<Type, EntityContextTypesInitializersPair> _objectSetInitializers =
            new SafetyDictionary<Type, EntityContextTypesInitializersPair>();

        // Used by the code below to create DbSet instances
        public static readonly MethodInfo MthSetRep = typeof(EntityContext).GetMethods().FirstOrDefault(s => s.Name == nameof(EntityContext.Set) && s.IsGenericMethod);

        private readonly EntityContext _context;
        private readonly EntityContextOptions _options;
        private readonly IContextService _service;

        // <summary>
        // Creates a set discovery service for the given derived context.
        // </summary>
        public EntityRepositoryDiscoveryService(EntityContext context, EntityContextOptions options)
        {
            _context = context;
            _options = options;
            _service = context.GetService<IContextService>();
        }

        #endregion

        #region Set discovery/processing

        // <summary>
        // Processes the given context type to determine the EntityRepository or IRepository
        // properties and collect root entity types from those properties.  Also, delegates are
        // created to initialize any of these properties that have public setters.
        // If the type has been processed previously in the app domain, then all this information
        // is returned from a cache.
        // </summary>
        // <returns> A dictionary of potential entity type to the list of the names of the properties that used the type. </returns>
        private Dictionary<Type, List<string>> GetSets()
        {
            EntityContextTypesInitializersPair setsInfo;
            var contextType = _context.GetType();
            if (!_objectSetInitializers.TryGetValue(contextType, out setsInfo))
            {
                // It is possible that multiple threads will enter this code and create the list
                // and the delegates.  However, the result will always be the same so we may, in
                // the rare cases in which this happens, do some work twice, but functionally the
                // outcome will be correct.

                var dbContextParam = Expression.Parameter(typeof(EntityContext), "dbContext");
                var initDelegates = new List<Action<EntityContext>>();

                var typeMap = new Dictionary<Type, List<string>>();

                var reposMaps = (from s in
                    contextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetIndexParameters().Length == 0
                        && p.DeclaringType != typeof(EntityContext))
                    let entityType = GetSetType(s.PropertyType)
                    where entityType != null
                    select new EntityRepositoryMapper { Property = s, EntityType = entityType }).ToList();

                _options.Initializers?.PreInitialize(new EntityContextPreInitializeContext(_context, _service, reposMaps));

                // Properties declared directly on DbContext such as Database are skipped
                foreach (var m in reposMaps)
                {
                    // We validate immediately because a DbSet/IDbSet must be of
                    // a valid entity type since otherwise you could never use an instance.
                    if (!m.EntityType.IsValidStructuralType())
                    {
                        //throw Error.InvalidEntityType(entityType);
                    }

                    if (!typeMap.TryGetValue(m.EntityType, out List<string> properties))
                    {
                        properties = new List<string>();
                        typeMap[m.EntityType] = properties;
                    }

                    properties.Add(m.Property.Name);

                    var setter = m.Property.GetSetMethod();
                    if (setter != null && setter.IsPublic)
                    {
                        var setMethod = MthSetRep.MakeGenericMethod(m.EntityType);

                        Expression expression = Expression.Call(dbContextParam, setMethod);
                        var pType = setter.GetParameters()[0].ParameterType;
                        if (pType != expression.Type)
                        {
                            expression = Expression.Convert(expression, pType);
                        }

                        var setExp = Expression.Call(
                            Expression.Convert(dbContextParam, contextType), setter, expression);

                        initDelegates.Add(
                            Expression.Lambda<Action<EntityContext>>(setExp, dbContextParam).Compile());
                    }
                }

                Action<EntityContext> initializer = dbContext =>
                    {
                        foreach (var initer in initDelegates)
                        {
                            initer(dbContext);
                        }
                    };

                setsInfo = new EntityContextTypesInitializersPair(typeMap, initializer);

                // If TryAdd fails it just means some other thread got here first, which is okay
                // since the end result is the same info anyway.
                _objectSetInitializers.TryAdd(_context.GetType(), setsInfo);
            }

            return setsInfo.EntityTypeToPropertyNameMap;
        }

        // <summary>
        // Calls the public setter on any property found to initialize it to a new instance of DbSet.
        // </summary>
        public void InitializeSets()
        {
            GetSets(); // Ensures sets have been discovered
            if (_objectSetInitializers[_context.GetType()] != null)
            {
                _objectSetInitializers[_context.GetType()].SetsInitializer(_context);
            }
        }

        #endregion

        #region Helpers

        // <summary>
        // Determines whether or not an instance of DbSet/ObjectSet can be assigned to a property of the given type.
        // </summary>
        // <param name="declaredType"> The type to check. </param>
        // <returns> The entity type of the DbSet/ObjectSet that can be assigned, or null if no set type can be assigned. </returns>
        private static Type GetSetType(Type declaredType)
        {
            if (!declaredType.IsArray)
            {
                var entityType = declaredType.GetEnumerableElementType();
                if (entityType != null)
                {
                    var setOfT = typeof(EntityRepository<>).MakeGenericType(entityType);
                    if (declaredType.IsAssignableFrom(setOfT))
                    {
                        return entityType;
                    }
                }
            }

            return null;
        }
        #endregion
    }

    public class EntityRepositoryMapper
    {
        public PropertyInfo Property { get; set; }

        public Type EntityType { get; set; }
    }
}
