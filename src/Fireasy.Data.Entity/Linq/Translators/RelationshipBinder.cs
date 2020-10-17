// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Properties;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Translates accesses to relationship members into projections or joins
    /// </summary>
    public class RelationshipBinder : DbExpressionVisitor
    {
        private TranslateContext _transContext;
        private Expression _currentFrom;

        public static Expression Bind(TranslateContext transContext, Expression expression)
        {
            return new RelationshipBinder { _transContext = transContext }.Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            var saveCurrentFrom = _currentFrom;
            _currentFrom = VisitSource(select.From);
            try
            {
                var where = Visit(select.Where);
                var orderBy = VisitOrderBy(select.OrderBy);
                var groupBy = VisitMemberAndExpressionList(select.GroupBy);
                var skip = Visit(select.Skip);
                var take = Visit(select.Take);
                var columns = VisitColumnDeclarations(select.Columns);

                if (_currentFrom != select.From
                    || where != select.Where
                    || orderBy != select.OrderBy
                    || groupBy != select.GroupBy
                    || take != select.Take
                    || skip != select.Skip
                    || columns != select.Columns
                    )
                {
                    return new SelectExpression(select.Alias, columns, _currentFrom, where, orderBy, groupBy, select.IsDistinct, skip, take, select.Segment, select.Having, select.IsReverse);
                }

                return select;
            }
            finally
            {
                _currentFrom = saveCurrentFrom;
            }
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            var source = Visit(m.Expression);
            IProperty property;

            // **fix** 解决无法返回两级以上关联对象的问题
            // var ex = source as EntityExpression
            // if (ex != null &&
            if ((property = PropertyUnity.GetProperty(m.Expression.Type, m.Member.Name)) != null &&
                property is RelationProperty)
            {
                var projection = (ProjectionExpression)Visit(QueryUtility.GetMemberExpression(_transContext, source, property));
                if (_currentFrom != null && m.Member.GetMemberType().GetEnumerableType() == null)
                {
                    // convert singleton associations directly to OUTER APPLY
                    projection = projection.AddOuterJoinTest();
                    var newFrom = new JoinExpression(JoinType.OuterApply, _currentFrom, projection.Select, null);
                    _currentFrom = newFrom;
                    return projection.Projector;
                }

                return projection;
            }

            var result = QueryBinder.BindMember(source, m.Member);

            if (result is MemberExpression mex &&
                mex.Member == m.Member &&
                mex.Expression == m.Expression)
            {
                return m;
            }

            return result;
        }
    }
}