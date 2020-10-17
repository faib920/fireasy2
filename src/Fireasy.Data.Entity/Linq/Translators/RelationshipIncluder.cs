// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Entity.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Adds relationship to query results depending on policy
    /// </summary>
    public class RelationshipIncluder : DbExpressionVisitor
    {
        private TranslateContext _transContext;
        private IQueryPolicy _policy;
        private ScopedDictionary<IProperty, bool> _includeScope = new ScopedDictionary<IProperty, bool>(null);

        public static Expression Include(TranslateContext transContext, Expression expression)
        {
            return new RelationshipIncluder { _transContext = transContext, _policy = transContext.QueryPolicy }.Visit(expression);
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            var projector = Visit(proj.Projector);
            return proj.Update(proj.Select, projector, proj.Aggregator);
        }

        protected override Expression VisitEntity(EntityExpression entity)
        {
            if (entity.Expression is MemberInitExpression init)
            {
                var save = _includeScope;
                _includeScope = new ScopedDictionary<IProperty, bool>(_includeScope);

                List<MemberBinding> newBindings = null;
                foreach (var property in PropertyUnity.GetRelatedProperties(entity.Type))
                {
                    PropertyInfo member;
                    if (property is IPropertyLazy && _policy.IsIncluded(member = property.Info.ReflectionInfo))
                    {
                        if (_includeScope.ContainsKey(property))
                        {
                            throw new NotSupportedException(string.Format("Cannot include '{0}.{1}' recursively.", property.Type, property.Name));
                        }

                        var me = QueryUtility.GetMemberExpression(_transContext, init, property);
                        if (newBindings == null)
                        {
                            newBindings = new List<MemberBinding>(init.Bindings);
                        }

                        newBindings.Add(Expression.Bind(member, me));
                    }
                }

                if (newBindings != null)
                {
                    entity = new EntityExpression(entity.Metadata, Expression.MemberInit(init.NewExpression, newBindings));
                }

                _includeScope = save;
            }

            return base.VisitEntity(entity);
        }
    }
}