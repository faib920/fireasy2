// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Properties;
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
        private IQueryPolicy policy;
        private ScopedDictionary<IProperty, bool> includeScope = new ScopedDictionary<IProperty, bool>(null);

        public static Expression Include(IQueryPolicy policy, Expression expression)
        {
            return new RelationshipIncluder(policy).Visit(expression);
        }

        public RelationshipIncluder(IQueryPolicy policy)
        {
            this.policy = policy;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            var projector = Visit(proj.Projector);
            return proj.Update(proj.Select, projector, proj.Aggregator);
        }

        protected override Expression VisitEntity(EntityExpression entity)
        {
            var init = entity.Expression as MemberInitExpression;
            if (init != null)
            {
                var save = includeScope;
                includeScope = new ScopedDictionary<IProperty, bool>(this.includeScope);

                List<MemberBinding> newBindings = null;
                foreach (var property in PropertyUnity.GetRelatedProperties(entity.Type))
                {
                    PropertyInfo member;
                    if (property is IPropertyLazy && policy.IsIncluded(member = property.Info.ReflectionInfo))
                    {
                        if (includeScope.ContainsKey(property))
                        {
                            throw new NotSupportedException(string.Format("Cannot include '{0}.{1}' recursively.", property.Type, property.Name));
                        }

                        var me = QueryUtility.GetMemberExpression(init, property);
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

                includeScope = save;
            }

            return base.VisitEntity(entity);
        }
    }
}