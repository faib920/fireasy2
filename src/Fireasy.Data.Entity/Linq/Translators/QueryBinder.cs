// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Dynamic;
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Entity.Query;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 用于将 Linq 查询表达式转换为 ELinq 表达式树表示。无法继承此类。
    /// </summary>
    internal sealed class QueryBinder : DbExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, Expression> _expMaps;
        private readonly Dictionary<Expression, GroupByInfo> _groupByMap;
        private Expression _root;
        private List<OrderExpression> _thenBys;
        private Expression _batchSource;
        private bool _isNoTracking = false;
        private bool _isQueryAsync = false;
        private TableAlias _alias;
        private TranslateContext _transContext;

        private QueryBinder(TranslateContext transContext, Expression root)
        {
            _expMaps = new Dictionary<ParameterExpression, Expression>();
            _groupByMap = new Dictionary<Expression, GroupByInfo>();
            _root = root;
            _transContext = transContext;
        }

        /// <summary>
        /// 将 Linq 查询表达式绑定为 ELinq 表示。
        /// </summary>
        /// <param name="expression">Linq 表达式。</param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        public static Expression Bind(TranslateContext transContext, Expression expression)
        {
            return new QueryBinder(transContext, expression).Visit(expression);
        }

        /// <summary>
        /// 访问 <see cref="MethodCallExpression"/> 的子级。
        /// </summary>
        /// <param name="node">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var funcName = TranslateUtils.GetCustomFunction(node.Method);
            if (!string.IsNullOrEmpty(funcName))
            {
                var arguments = Visit(node.Arguments);
                return new FunctionExpression(node.Method.ReturnType, funcName, arguments);
            }

            var binder = TranslateUtils.GetMethodBinder(node.Method);
            if (binder != null)
            {
                return binder.Bind(new MethodCallBindContext(this, node, _transContext.SyntaxProvider));
            }

            if (node.Method.DeclaringType == typeof(Queryable) ||
                node.Method.DeclaringType == typeof(Enumerable))
            {
                return BindQueryableMethod(node);
            }
            else if (typeof(ITreeRepository).IsAssignableFrom(node.Method.DeclaringType))
            {
                return BindTreeRepositoryMethod(node);
            }
            else if (typeof(IList).IsAssignableFrom(node.Method.DeclaringType))
            {
                if (node.Method.Name == nameof(IList.Contains))
                {
                    return BindContains(node.Object, node.Arguments[0], node == _root);
                }
            }
            else if (node.Method.DeclaringType == typeof(GenericExtension))
            {
                switch (node.Method.Name)
                {
                    case nameof(GenericExtension.To):
                        return BindTo(node.Type, node.Arguments[0]);
                }
            }
            else if (node.Method.DeclaringType == typeof(Extensions))
            {
                var isAsync = node.Method.Name.EndsWith("Async");
                switch (node.Method.Name)
                {
                    case nameof(Extensions.Segment):
                        return BindSegment(node.Arguments[0], node.Arguments[1]);
                    case nameof(Extensions.RemoveWhere):
                    case nameof(Extensions.RemoveWhereAsync):
                        return BindDelete(node.Arguments[0], GetLambda(node.Arguments[1]), node.Arguments[2], isAsync);
                    case nameof(Extensions.UpdateWhere):
                    case nameof(Extensions.UpdateWhereAsync):
                    case nameof(Extensions.UpdateWhereByCalculator):
                    case nameof(Extensions.UpdateWhereByCalculatorAsync):
                        return BindUpdate(node.Arguments[0], node.Arguments[1], GetLambda(node.Arguments[2]), isAsync);
                    case nameof(Extensions.CreateEntity):
                    case nameof(Extensions.CreateEntityAsync):
                        return BindInsert(node.Arguments[0], node.Arguments[1], isAsync);
                    case nameof(Extensions.BatchOperate):
                    case nameof(Extensions.BatchOperateAsync):
                        _batchSource = node.Arguments[0];
                        return BindBatch(node.Arguments[0], node.Arguments[1], GetLambda(node.Arguments[2]), node.Arguments[3], isAsync);
                    case nameof(Extensions.Extend):
                        return BindExtend(node.Arguments[0], GetLambda(node.Arguments[1]));
                    case nameof(Extensions.ExtendAs):
                    case nameof(Extensions.ExtendGenericAs):
                        return BindExtendAs(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]));
                    case nameof(Extensions.AsNoTracking):
                        _isNoTracking = true;
                        return Visit(node.Arguments[0]);
                    case nameof(Extensions.CacheParsing):
                    case nameof(Extensions.CacheExecution):
                        return Visit(node.Arguments[0]);
                    case nameof(Extensions.FirstOrDefaultAsync):
                    case nameof(Extensions.LastOrDefaultAsync):
                    case nameof(Extensions.SingleOrDefaultAsync):
                        var kind = node.Method.Name.Replace("Async", string.Empty);
                        if (node.Arguments.Count == 2)
                        {
                            return BindFirst(node.Arguments[0], null, kind, node == _root, true);
                        }
                        else if (node.Arguments.Count == 3)
                        {
                            return BindFirst(node.Arguments[0], GetLambda(node.Arguments[1]), kind, node == _root, true);
                        }
                        break;
                    case nameof(Extensions.AnyAsync):
                    case nameof(Extensions.AllAsync):
                        return BindAnyAll(node.Arguments[0], node.Method, node.Arguments.Count == 3 ? GetLambda(node.Arguments[1]) : null, node == _root, true);
                    case nameof(Extensions.CountAsync):
                    case nameof(Extensions.MinAsync):
                    case nameof(Extensions.MaxAsync):
                    case nameof(Extensions.SumAsync):
                    case nameof(Extensions.AverageAsync):
                        return BindAggregate(node.Arguments[0], node.Method, node.Arguments.Count == 3 ? GetLambda(node.Arguments[1]) : null, node == _root, true);
                    case nameof(Extensions.ToListAsync):
                        _isQueryAsync = true;
                        return Visit(node.Arguments[0]);
                    case nameof(Extensions.Where):
                        return BindWhere(node.Type, node.Arguments[0], node.Arguments[1], node.Arguments[2]);
                }
            }
            else if (typeof(IRepository).IsAssignableFrom(node.Method.DeclaringType))
            {
                var isAsync = node.Method.Name.EndsWith("Async");
                switch (node.Method.Name)
                {
                    case nameof(IRepository.Delete):
                    case nameof(IRepository.DeleteAsync):
                        var predicate1 = GetLambda(node.Arguments[0]);
                        if (predicate1 != null)
                        {
                            return BindDelete(_batchSource, predicate1, node.Arguments[1], isAsync);
                        }
                        else
                        {
                            return BindDelete(_batchSource, (ParameterExpression)node.Arguments[0], node.Arguments[1], isAsync);
                        }
                    case nameof(IRepository.Update):
                    case nameof(IRepository.UpdateAsync):
                        var predicate2 = node.Arguments.Count > 1 ? GetLambda(node.Arguments[1]) : null;
                        return BindUpdate(_batchSource, node.Arguments[0], predicate2, isAsync);
                    case nameof(IRepository.Insert):
                    case nameof(IRepository.InsertAsync):
                        return BindInsert(_batchSource, node.Arguments[0], isAsync);
                }
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// 访问 <see cref="ConstantExpression"/>。
        /// </summary>
        /// <param name="c">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = null;
            if (IsQueryable(c) && (q = (IQueryable)c.Value).Expression != null)
            {
                var rowType = ReflectionCache.GetMember("EnumerableElementType", c.Type, k => k.GetEnumerableElementType());
                if (typeof(IEntity).IsAssignableFrom(rowType) &&
                    q.Expression.NodeType == ExpressionType.Constant)
                {
                    return VisitSequence(QueryUtility.GetTableQuery(_transContext, EntityMetadataUnity.GetEntityMetadata(rowType), _isNoTracking, _isQueryAsync));
                }
                else if (q.Expression.NodeType == ExpressionType.Constant)
                {
                    // assume this is also a table via some other implementation of IQueryable
                    return VisitSequence(QueryUtility.GetTableQuery(_transContext, EntityMetadataUnity.GetEntityMetadata(q.ElementType), _isNoTracking, _isQueryAsync));
                }
                else
                {
                    var translator = _transContext.TranslateProvider;
                    var pev = Common.Linq.Expressions.PartialEvaluator.Eval(q.Expression, translator.CanBeEvaluatedLocally);
                    return Visit(pev);
                }
            }

            return c;
        }

        /// <summary>
        /// 访问 <see cref="ParameterExpression"/>。
        /// </summary>
        /// <param name="p">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            return _expMaps.TryGetValue(p, out Expression e) ? e : p;
        }

        /// <summary>
        /// 访问 <see cref="InvocationExpression"/>。
        /// </summary>
        /// <param name="iv">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            if (iv.Expression is LambdaExpression lambda)
            {
                for (int i = 0, n = lambda.Parameters.Count; i < n; i++)
                {
                    _expMaps[lambda.Parameters[i]] = iv.Arguments[i];
                }

                return Visit(lambda.Body);
            }

            return base.VisitInvocation(iv);
        }

        /// <summary>
        /// 访问 <see cref="MemberExpression"/>。
        /// </summary>
        /// <param name="m">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null
                && m.Expression.NodeType == ExpressionType.Parameter
                && !_expMaps.ContainsKey((ParameterExpression)m.Expression)
                && IsQueryable(m))
            {
                return VisitSequence(QueryUtility.GetTableQuery(_transContext, EntityMetadataUnity.GetEntityMetadata(m.Type.GetEnumerableElementType()), _isNoTracking, _isQueryAsync));
            }

            if (m.Member.DeclaringType.IsNullableType() && m.Member.Name == nameof(Nullable<int>.HasValue))
            {
                return Visit(Expression.NotEqual(m.Expression, Expression.Constant(null)));
            }

            var source = Visit(m.Expression);
            if (IsAggregate(m.Member) && IsRemoteQuery(source))
            {
                return BindAggregate(m.Expression, m.Member, null, m == _root, false);
            }

            var result = BindMember(source, m.Member);
            if (result is MemberExpression mex &&
                mex.Member == m.Member &&
                mex.Expression == m.Expression)
            {
                return m;
            }

            return result;
        }

        protected override Expression VisitUpdate(UpdateCommandExpression update)
        {
            var assignments = update.Assignments.Select(s => new ColumnAssignment(s.Column, Visit(s.Expression))).ToArray();
            return update.Update(update.Table, update.Where, assignments);
        }

        /// <summary>
        /// 获取 Lambda 表达式。
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取下一个表别名。
        /// </summary>
        /// <returns></returns>
        private TableAlias GetNextAlias()
        {
            return new TableAlias();
        }

        /// <summary>
        /// 影射表达式中的所有列表达式。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="newAlias"></param>
        /// <param name="existingAliases"></param>
        /// <returns></returns>
        private ProjectedColumns ProjectColumns(Expression expression, TableAlias newAlias, params TableAlias[] existingAliases)
        {
            return ColumnProjector.ProjectColumns(QueryUtility.CanBeColumnExpression, expression, null, newAlias, existingAliases);
        }

        private ProjectionExpression VisitSequence(Expression source)
        {
            return ConvertToSequence(base.Visit(source)) as ProjectionExpression;
        }

        private Expression ConvertToSequence(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Quote)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            switch (expr.NodeType)
            {
                case (ExpressionType)DbExpressionType.Projection:
                    return (ProjectionExpression)expr;
                case ExpressionType.New:
                    var nex = (NewExpression)expr;
                    if (expr.Type.IsGenericType &&
                        expr.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
                    {
                        return (ProjectionExpression)nex.Arguments[1];
                    }
                    goto default;
                case ExpressionType.MemberAccess:
                    var bound = BindRelationshipProperty((MemberExpression)expr);
                    if (bound.NodeType != ExpressionType.MemberAccess)
                    {
                        return ConvertToSequence(bound);
                    }
                    goto default;
                case ExpressionType.Constant:
                    // 如果使用两个以上的序列嵌套查询，并且嵌套的序列定义成了变量，如
                    // var products = Products.Select(p => p.Product_Name);
                    // var orders = Orders.Where(v => products.Contains(v.CustomerID)).Select(o => o.CustomerID);
                    // var customers = Customers.Where(c => orders.Contains(c.CustomerID));
                    // 而非
                    // var customers = Customers.Where(c => Orders.Where(v => Products.Select(p => p.Product_Name).Contains(v.CustomerID)).Select(o => o.CustomerID).Contains(c.CustomerID))
                    var constExp = expr as ConstantExpression;
                    if (constExp.Value is IQueryable queryable)
                    {
                        return (ProjectionExpression)Bind(_transContext, queryable.Expression);
                    }
                    goto default;
                default:
                    var n = GetNewExpression(expr);
                    if (n != null)
                    {
                        expr = n;
                        goto case ExpressionType.New;
                    }
                    return expr;
            }
        }

        private Expression BindRelationshipProperty(MemberExpression mex)
        {
            if (mex.Expression is EntityExpression ex)
            {
                var property = PropertyUnity.GetProperty(ex.Type, mex.Member.Name);
                if (property is RelationProperty)
                {
                    return QueryUtility.GetMemberExpression(_transContext, mex.Expression, property);
                }
            }

            return mex;
        }

        /// <summary>
        /// 访问 <see cref="Expression"/>。
        /// </summary>
        /// <param name="exp">要访问的表达式。</param>
        /// <returns></returns>
        public override Expression Visit(Expression exp)
        {
            var result = base.Visit(exp);

            if (result != null)
            {
                var expectedType = exp.Type;
                if (result is ProjectionExpression projection &&
                    projection.Aggregator == null &&
                    !expectedType.IsAssignableFrom(projection.Type))
                {
                    var aggregator = QueryUtility.GetAggregator(expectedType, projection.Type);
                    if (aggregator != null)
                    {
                        return new ProjectionExpression(projection.Select, projection.Projector, aggregator, _isQueryAsync, _isNoTracking);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 绑定 Where 子句。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            _expMaps[predicate.Parameters[0]] = projection.Projector;
            var where = Visit(predicate.Body);

            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, where),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression BindWhere(Type resultType, Expression source, Expression condition, Expression parameters)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            var sql = ((ConstantExpression)condition).Value.ToString();
            List<NamedValueExpression> values = null;
            if (parameters != null && parameters is ConstantExpression constExp && constExp.Value is ParameterCollection pars)
            {
                values = pars.Select(s => new NamedValueExpression(s.ParameterName, Expression.Constant(s.Value))).ToList();
            }

            var where = new SqlExpression(sql, values);

            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, where),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression BindReverse(Expression source)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null).Reverse(true),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        /// <summary>
        /// 绑定 Select 子句。
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            _expMaps[selector.Parameters[0]] = projection.Projector;

            _alias = projection.Select.Alias;
            var expression = Visit(selector.Body);
            _alias = null;

            var alias = GetNextAlias();
            var pc = ProjectColumns(expression, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression BindSelectMany(Type resultType, Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            _expMaps[collectionSelector.Parameters[0]] = projection.Projector;

            var collection = collectionSelector.Body;

            var defaultIfEmpty = false;
            if (collection is MethodCallExpression mcs &&
                mcs.Method.Name == nameof(Queryable.DefaultIfEmpty) &&
                mcs.Arguments.Count == 1 &&
                (mcs.Method.DeclaringType == typeof(Queryable) || mcs.Method.DeclaringType == typeof(Enumerable)))
            {
                collection = mcs.Arguments[0];
                defaultIfEmpty = true;
            }

            var collectionProjection = VisitSequence(collection);
            if (collectionProjection == null)
            {
                return null;
            }

            var isTable = collectionProjection.Select.From is TableExpression;
            var joinType = isTable ? JoinType.CrossJoin : defaultIfEmpty ? JoinType.OuterApply : JoinType.CrossApply;
            if (joinType == JoinType.OuterApply)
            {
                collectionProjection = collectionProjection.AddOuterJoinTest();
            }

            var join = new JoinExpression(joinType, projection.Select, collectionProjection.Select, null);

            var alias = GetNextAlias();
            ProjectedColumns pc;
            if (resultSelector == null)
            {
                pc = ProjectColumns(collectionProjection.Projector, alias, projection.Select.Alias, collectionProjection.Select.Alias);
            }
            else
            {
                _expMaps[resultSelector.Parameters[0]] = projection.Projector;
                _expMaps[resultSelector.Parameters[1]] = collectionProjection.Projector;
                var result = Visit(resultSelector.Body);
                pc = ProjectColumns(result, alias, projection.Select.Alias, collectionProjection.Select.Alias);
            }

            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, join, null),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression BindJoin(Type resultType, Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector)
        {
            var joinType = GetJoinType(ref outerSource, ref innerSource);
            var outerProjection = VisitSequence(outerSource);
            var innerProjection = VisitSequence(innerSource);

            _expMaps[outerKey.Parameters[0]] = outerProjection.Projector;
            var outerKeyExpr = GroupKeyReplacer.Replace(Visit(outerKey.Body));
            _expMaps[innerKey.Parameters[0]] = innerProjection.Projector;
            var innerKeyExpr = GroupKeyReplacer.Replace(Visit(innerKey.Body));
            _expMaps[resultSelector.Parameters[0]] = outerProjection.Projector;
            _expMaps[resultSelector.Parameters[1]] = innerProjection.Projector;

            _alias = outerProjection.Select.Alias;
            var resultExpr = Visit(resultSelector.Body);
            _alias = null;

            var join = new JoinExpression(joinType, outerProjection.Select, innerProjection.Select, outerKeyExpr.Equal(innerKeyExpr));

            var alias = GetNextAlias();
            var pc = ProjectColumns(resultExpr, alias, outerProjection.Select.Alias, innerProjection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, join, null),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression ReplaceGroupKey(Expression expression)
        {
            return expression;
        }

        private Expression BindIntersect(Expression outerQueryable, Expression innerQueryable, bool negate)
        {
            // SELECT * FROM outer WHERE EXISTS(SELECT * FROM inner WHERE inner = outer))
            var outerProjection = VisitSequence(outerQueryable);
            var innerProjection = VisitSequence(innerQueryable);

            Expression exists = new ExistsExpression(
                new SelectExpression(new TableAlias(), null, innerProjection.Select,
                innerProjection.Projector.Equal(outerProjection.Projector))
                );

            if (negate)
            {
                exists = Expression.Not(exists);
            }

            var alias = GetNextAlias();
            var pc = ProjectColumns(outerProjection.Projector, alias, outerProjection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, outerProjection.Select, exists),
                pc.Projector, outerProjection.Aggregator, _isQueryAsync, _isNoTracking
                );
        }

        private JoinType GetJoinType(ref Expression outerSource, ref Expression innerSource)
        {
            if (outerSource.NodeType == ExpressionType.Call)
            {
                var callExp = outerSource as MethodCallExpression;
                if (callExp.Method.Name == nameof(Queryable.DefaultIfEmpty))
                {
                    outerSource = callExp.Arguments[0];
                    return JoinType.RightOuter;
                }
            }

            if (innerSource.NodeType == ExpressionType.Call)
            {
                var callExp = innerSource as MethodCallExpression;
                if (callExp.Method.Name == nameof(Queryable.DefaultIfEmpty))
                {
                    innerSource = callExp.Arguments[0];
                    return JoinType.LeftOuter;
                }
            }

            return JoinType.InnerJoin;
        }

        private Expression BindOrderBy(Type resultType, Expression source, LambdaExpression orderSelector, OrderType orderType)
        {
            var myThenBys = _thenBys;
            _thenBys = null;
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            _expMaps[orderSelector.Parameters[0]] = projection.Projector;
            var orderings = GetOrderExpressions(orderType, orderSelector.Body);

            if (myThenBys != null)
            {
                for (var i = myThenBys.Count - 1; i >= 0; i--)
                {
                    var tb = myThenBys[i];
                    var lambda = (LambdaExpression)tb.Expression;
                    _expMaps[lambda.Parameters[0]] = projection.Projector;
                    orderings.AddRange(GetOrderExpressions(tb.OrderType, lambda.Body));
                }
            }

            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null, orderings.AsReadOnly(), null, null),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        /// <summary>
        /// 获取排序表达式列表。
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private List<OrderExpression> GetOrderExpressions(OrderType orderType, Expression expression)
        {
            //使用new表达式指定多个字段排序
            if (expression.NodeType == ExpressionType.New)
            {
                var newExp = expression as NewExpression;

                return newExp.Arguments.Select(s => new OrderExpression(orderType, Visit(s))).ToList();
            }
            else
            {
                return new List<OrderExpression> { new OrderExpression(orderType, Visit(expression)) };
            }
        }

        private Expression BindThenBy(Expression source, LambdaExpression orderSelector, OrderType orderType)
        {
            if (_thenBys == null)
            {
                _thenBys = new List<OrderExpression>();
            }

            _thenBys.Add(new OrderExpression(orderType, orderSelector));
            return Visit(source);
        }

        private Expression BindGroupBy(Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            _expMaps[keySelector.Parameters[0]] = projection.Projector;
            var keyExpr = Visit(keySelector.Body);

            var elemExpr = projection.Projector;
            if (elementSelector != null)
            {
                _expMaps[elementSelector.Parameters[0]] = projection.Projector;
                elemExpr = Visit(elementSelector.Body);
            }

            // Use ProjectColumns to get group-by expressions from key expression
            var keyProjection = ProjectColumns(keyExpr, projection.Select.Alias, projection.Select.Alias);
            var groupExprs = keyProjection.Columns.Select(c => c.Expression);

            // make duplicate of source query as basis of element subquery by visiting the source again
            var subqueryBasis = VisitSequence(source);
            if (subqueryBasis == null)
            {
                return null;
            }

            // recompute key columns for group expressions relative to subquery (need these for doing the correlation predicate)
            _expMaps[keySelector.Parameters[0]] = subqueryBasis.Projector;
            var subqueryKey = Visit(keySelector.Body);

            // use same projection trick to get group-by expressions based on subquery
            var subqueryKeyPC = ProjectColumns(subqueryKey, subqueryBasis.Select.Alias, subqueryBasis.Select.Alias);
            var subqueryGroupExprs = subqueryKeyPC.Columns.Select(c => c.Expression);
            var subqueryCorrelation = BuildPredicateWithNullsEqual(subqueryGroupExprs, groupExprs);

            // compute element based on duplicated subquery
            var subqueryElemExpr = subqueryBasis.Projector;
            if (elementSelector != null)
            {
                _expMaps[elementSelector.Parameters[0]] = subqueryBasis.Projector;
                subqueryElemExpr = Visit(elementSelector.Body);
            }

            // build subquery that projects the desired element
            var elementAlias = GetNextAlias();
            var elementPC = ProjectColumns(subqueryElemExpr, elementAlias, subqueryBasis.Select.Alias);
            var elementSubquery =
                new ProjectionExpression(
                    new SelectExpression(elementAlias, elementPC.Columns, subqueryBasis.Select, subqueryCorrelation),
                    elementPC.Projector, _isQueryAsync, _isNoTracking
                    );

            var alias = GetNextAlias();

            // make it possible to tie aggregates back to this group-by
            var info = new GroupByInfo(alias, elemExpr);
            _groupByMap.Add(elementSubquery, info);

            Expression resultExpr;
            if (resultSelector != null)
            {
                var saveGroupElement = m_currentGroupElement;
                m_currentGroupElement = elementSubquery;
                // compute result expression based on key & element-subquery
                _expMaps[resultSelector.Parameters[0]] = keyProjection.Projector;
                _expMaps[resultSelector.Parameters[1]] = elementSubquery;
                resultExpr = Visit(resultSelector.Body);
                m_currentGroupElement = saveGroupElement;
            }
            else
            {
                // result must be IGrouping<K,E>
                var gpConstructor = ReflectionCache.GetMember("GroupingConstructor", new[] { keyExpr.Type, subqueryElemExpr.Type }, pars => typeof(Grouping<,>).MakeGenericType(pars).GetConstructors()[0]);
                resultExpr =
                    Expression.New(
                        gpConstructor,
                        new[] { keyExpr, elementSubquery }
                        );

                resultExpr = Expression.Convert(resultExpr, gpConstructor.DeclaringType);
            }

            var pc = ProjectColumns(resultExpr, alias, projection.Select.Alias);

            // make it possible to tie aggregates back to this group-by
            var newResult = GetNewExpression(pc.Projector);
            if (newResult != null && newResult.Type.IsGenericType && newResult.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
            {
                Expression projectedElementSubquery = newResult.Arguments[1];
                _groupByMap.Add(projectedElementSubquery, info);
            }

            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null, null, groupExprs, null),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression BindGroupJoin(MethodInfo groupJoinMethod, Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector)
        {
            // A database will treat this no differently than a SelectMany w/ result selector, so just use that translation instead
            var args = groupJoinMethod.GetGenericArguments();

            var outerProjection = VisitSequence(outerSource);
            if (outerProjection == null)
            {
                return null;
            }

            _expMaps[outerKey.Parameters[0]] = outerProjection.Projector;
            var predicateLambda = Expression.Lambda(innerKey.Body.Equal(outerKey.Body), innerKey.Parameters[0]);
            var callToWhere = Expression.Call(typeof(Enumerable), "Where", new Type[] { args[1] }, innerSource, predicateLambda);
            var group = Visit(callToWhere);

            _expMaps[resultSelector.Parameters[0]] = outerProjection.Projector;
            _expMaps[resultSelector.Parameters[1]] = group;
            var resultExpr = Visit(resultSelector.Body);

            var alias = GetNextAlias();
            var pc = ProjectColumns(resultExpr, alias, outerProjection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, outerProjection.Select, null),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression BuildPredicateWithNullsEqual(IEnumerable<Expression> source1, IEnumerable<Expression> source2)
        {
            var enum1 = source1.GetEnumerator();
            var enum2 = source2.GetEnumerator();

            Expression result = null;
            while (enum1.MoveNext() && enum2.MoveNext())
            {
                Expression compare =
                    Expression.Or(
                        Expression.And(new IsNullExpression(enum1.Current), new IsNullExpression(enum2.Current)),
                        enum1.Current.Equal(enum2.Current)
                        );
                result = (result == null) ? compare : Expression.And(result, compare);
            }

            enum1.Dispose();
            enum2.Dispose();

            return result;
        }

        private Expression m_currentGroupElement;

        private class GroupByInfo
        {
            internal TableAlias Alias { get; }

            internal Expression Element { get; }

            internal GroupByInfo(TableAlias alias, Expression element)
            {
                Alias = alias;
                Element = element;
            }
        }

        private AggregateType GetAggregateType(string name)
        {
            switch (name)
            {
                case nameof(Enumerable.Count):
                case nameof(Enumerable.LongCount):
                case nameof(Extensions.CountAsync):
                    return AggregateType.Count;
                case nameof(Enumerable.Min):
                case nameof(Extensions.MinAsync):
                    return AggregateType.Min;
                case nameof(Enumerable.Max):
                case nameof(Extensions.MaxAsync):
                    return AggregateType.Max;
                case nameof(Enumerable.Sum):
                case nameof(Extensions.SumAsync):
                    return AggregateType.Sum;
                case nameof(Enumerable.Average):
                case nameof(Extensions.AverageAsync):
                    return AggregateType.Average;
                default: throw new TranslateException(null, new Exception(SR.GetString(SRKind.UnknowAggregateType, name)));
            }
        }

        private bool IsAggregate(MemberInfo member)
        {
            if (member is MethodInfo method)
            {
                if (method.DeclaringType == typeof(Queryable)
                    || method.DeclaringType == typeof(Enumerable))
                {
                    switch (method.Name)
                    {
                        case nameof(Enumerable.Count):
                        case nameof(Enumerable.LongCount):
                        case nameof(Enumerable.Sum):
                        case nameof(Enumerable.Min):
                        case nameof(Enumerable.Max):
                        case nameof(Enumerable.Average):
                            return true;
                    }
                }
            }
            else if (member is PropertyInfo property
                && property.Name == nameof(Enumerable.Count)
                && typeof(IEnumerable).IsAssignableFrom(property.DeclaringType))
            {
                return true;
            }

            return false;
        }

        private bool IsRemoteQuery(Expression expression)
        {
            if ((int)expression.NodeType >= 1000)
            {
                return true;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return IsRemoteQuery(((MemberExpression)expression).Expression);
                case ExpressionType.Call:
                    var mc = (MethodCallExpression)expression;
                    if (mc.Object != null)
                    {
                        return IsRemoteQuery(mc.Object);
                    }
                    else if (mc.Arguments.Count > 0)
                    {
                        return IsRemoteQuery(mc.Arguments[0]);
                    }
                    break;
            }

            return false;
        }

        private Type GetTrueUnderlyingType(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression.Type;
        }

        private bool HasPredicateArg(AggregateType aggregateType)
        {
            return aggregateType == AggregateType.Count;
        }

        private Expression BindQueryableMethod(MethodCallExpression node)
        {
            Expression newExp = null;
            switch (node.Method.Name)
            {
                case nameof(Queryable.Where):
                    newExp = BindWhere(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]));
                    break;
                case nameof(Queryable.Select):
                    newExp = BindSelect(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]));
                    break;
                case nameof(Queryable.SelectMany):
                    newExp = BindSelectMany(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]), node.Arguments.Count == 3 ? GetLambda(node.Arguments[2]) : null);
                    break;
                case nameof(Queryable.Join):
                    newExp = BindJoin(node.Type, node.Arguments[0], node.Arguments[1], GetLambda(node.Arguments[2]), GetLambda(node.Arguments[3]), GetLambda(node.Arguments[4]));
                    break;
                case nameof(Queryable.OrderBy):
                    newExp = BindOrderBy(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]), OrderType.Ascending);
                    break;
                case nameof(Queryable.OrderByDescending):
                    newExp = BindOrderBy(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]), OrderType.Descending);
                    break;
                case nameof(Queryable.ThenBy):
                    newExp = BindThenBy(node.Arguments[0], GetLambda(node.Arguments[1]), OrderType.Ascending);
                    break;
                case nameof(Queryable.ThenByDescending):
                    newExp = BindThenBy(node.Arguments[0], GetLambda(node.Arguments[1]), OrderType.Descending);
                    break;
                case nameof(Queryable.GroupBy):
                    switch (node.Arguments.Count)
                    {
                        case 2:
                            newExp = BindGroupBy(node.Arguments[0], GetLambda(node.Arguments[1]), null, null);
                            break;
                        case 3:
                            var lambda1 = GetLambda(node.Arguments[1]);
                            var lambda2 = GetLambda(node.Arguments[2]);
                            switch (lambda2.Parameters.Count)
                            {
                                case 1:
                                    newExp = BindGroupBy(node.Arguments[0], lambda1, lambda2, null);
                                    break;
                                case 2:
                                    newExp = BindGroupBy(node.Arguments[0], lambda1, null, lambda2);
                                    break;
                            }
                            break;
                        case 4:
                            newExp = BindGroupBy(node.Arguments[0], GetLambda(node.Arguments[1]), GetLambda(node.Arguments[2]), GetLambda(node.Arguments[3]));
                            break;
                    }
                    break;
                case nameof(Queryable.GroupJoin):
                    if (node.Arguments.Count == 5)
                    {
                        newExp = BindGroupJoin(node.Method, node.Arguments[0], node.Arguments[1], GetLambda(node.Arguments[2]), GetLambda(node.Arguments[3]), GetLambda(node.Arguments[4]));
                    }
                    break;
                case nameof(Queryable.Count):
                case nameof(Queryable.LongCount):
                case nameof(Queryable.Min):
                case nameof(Queryable.Max):
                case nameof(Queryable.Sum):
                case nameof(Queryable.Average):
                    newExp = BindAggregate(node.Arguments[0], node.Method, node.Arguments.Count == 2 ? GetLambda(node.Arguments[1]) : null, node == _root, false);
                    break;
                case nameof(Queryable.Reverse):
                    newExp = BindReverse(node.Arguments[0]);
                    break;
                case nameof(Queryable.Distinct):
                    if (node.Arguments.Count == 1)
                    {
                        newExp = BindDistinct(node.Arguments[0]);
                    }
                    break;
                case nameof(Queryable.Except):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindIntersect(node.Arguments[0], node.Arguments[1], true);
                    }
                    break;
                case nameof(Queryable.Intersect):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindIntersect(node.Arguments[0], node.Arguments[1], false);
                    }
                    break;
                case nameof(Queryable.Cast):
                    if (node.Arguments.Count == 1)
                    {
                        newExp = BindCast(node.Arguments[0], node.Method.GetGenericArguments()[0]);
                    }
                    break;
                case nameof(Queryable.Skip):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindSkip(node.Arguments[0], node.Arguments[1]);
                    }
                    break;
                case nameof(Queryable.Take):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindTake(node.Arguments[0], node.Arguments[1]);
                    }
                    break;
                case nameof(Queryable.First):
                case nameof(Queryable.FirstOrDefault):
                case nameof(Queryable.Single):
                case nameof(Queryable.SingleOrDefault):
                case nameof(Queryable.Last):
                case nameof(Queryable.LastOrDefault):
                    if (node.Arguments.Count == 1)
                    {
                        newExp = BindFirst(node.Arguments[0], null, node.Method.Name, node == _root);
                    }
                    else if (node.Arguments.Count == 2)
                    {
                        newExp = BindFirst(node.Arguments[0], GetLambda(node.Arguments[1]), node.Method.Name, node == _root);
                    }
                    break;
                case nameof(Queryable.Any):
                    newExp = BindAnyAll(node.Arguments[0], node.Method, node.Arguments.Count == 2 ? GetLambda(node.Arguments[1]) : null, node == _root, false);
                    break;
                case nameof(Queryable.All):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindAnyAll(node.Arguments[0], node.Method, GetLambda(node.Arguments[1]), node == _root, false);
                    }
                    break;
                case nameof(Queryable.Contains):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindContains(node.Arguments[0], node.Arguments[1], node == _root);
                    }
                    break;
                case nameof(Queryable.DefaultIfEmpty):
                    newExp = Visit(node.Arguments[0]);
                    break;
            }

            if (newExp == null)
            {
                var arguments = node.Arguments.Select(s => Visit(s)).ToArray();
                return node.Update(node.Object, arguments);
            }

            return newExp;
        }

        /// <summary>
        /// 改变 <see cref="ITreeRepository"/> 的方法。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private Expression BindTreeRepositoryMethod(MethodCallExpression m)
        {
            if (m.Method.Name == nameof(ITreeRepository.HasChildren))
            {
                return BindHasChildren(m);
            }

            return m;
        }

        private Expression BindHasChildren(MethodCallExpression m)
        {
            var eleType = (m.Arguments[0] as ParameterExpression).Type;
            var parExp = Expression.Parameter(eleType, "s");

            var metadata = EntityMetadataUnity.GetEntityMetadata(eleType).EntityTree;
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
            var noExp = Expression.MakeMemberAccess(m.Arguments[0], metadata.InnerSign.Info.ReflectionInfo);
            var mthConcat = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });
            var condition = (Expression)Expression.Call(typeof(StringExtension),
                nameof(StringExtension.Like), null, memberExp,
                Expression.Call(null, mthConcat, noExp, Expression.Constant(new string('_', metadata.SignLength))));

            var lambda = GetLambda(m.Arguments[1]);
            if (lambda != null)
            {
                var exp = DbExpressionReplacer.Replace(lambda.Body, lambda.Parameters[0], parExp);
                condition = condition.And(exp);
            }

            var deleKey = metadata.Parent.DeleteProperty;
            if (deleKey != null)
            {
                condition = Expression.And(condition, Expression.Equal(Expression.MakeMemberAccess(parExp, deleKey.Info.ReflectionInfo), Expression.Constant(0.ToType(deleKey.Type))));
            }

            lambda = Expression.Lambda(condition, parExp);
            var query = CreateQuery(eleType, m.Object);
            condition = Expression.Call(typeof(Enumerable), nameof(Enumerable.Count), new[] { eleType }, query.Expression, lambda);

            return Visit(Expression.GreaterThan(condition, Expression.Constant(0)));
        }

        private IQueryable CreateQuery(Type eleType, Expression exp)
        {
            var persister = (exp as ConstantExpression).Value;
            var provider = (persister as IQueryProviderAware).Provider;
            var newType = ReflectionCache.GetMember("QueryableType", eleType, k => typeof(QuerySet<>).MakeGenericType(k));
            return (IQueryable)newType.New(provider);
        }

        private Expression BindAggregate(Expression source, MemberInfo member, LambdaExpression argument, bool isRoot, bool isAsync)
        {
            var returnType = member.GetMemberType();
            var valueType = isAsync ? returnType.GetGenericArguments()[0] : returnType;
            var aggType = GetAggregateType(member.Name);
            var hasPredicateArg = HasPredicateArg(aggType);
            var isDistinct = false;
            var argumentWasPredicate = false;

            // check for distinct
            if (source is MethodCallExpression mcs && !hasPredicateArg && argument == null)
            {
                if (mcs.Method.Name == nameof(Queryable.Distinct) && mcs.Arguments.Count == 1 &&
                    (mcs.Method.DeclaringType == typeof(Queryable) || mcs.Method.DeclaringType == typeof(Enumerable)) &&
                    _transContext.SyntaxProvider.SupportDistinctInAggregates)
                {
                    source = mcs.Arguments[0];
                    isDistinct = true;
                }
            }

            if (argument != null && hasPredicateArg)
            {
                // convert query.Count(predicate) into query.Where(predicate).Count()
                var type = typeof(IQueryable).IsAssignableFrom(source.Type) ? typeof(Queryable) : typeof(Enumerable);
                source = Expression.Call(type, nameof(Queryable.Where), new Type[] { source.Type.GetEnumerableElementType() }, source, argument);
                argument = null;
                argumentWasPredicate = true;
            }

            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            Expression argExpr = null;
            if (argument != null)
            {
                _expMaps[argument.Parameters[0]] = projection.Projector;
                argExpr = Visit(argument.Body);
            }
            else if (!hasPredicateArg)
            {
                argExpr = projection.Projector;
            }

            var alias = GetNextAlias();
            var aggExpr = new AggregateExpression(valueType, aggType, argExpr, isDistinct);
            var select = new SelectExpression(alias, new[] { new ColumnDeclaration(string.Empty, aggExpr) }, projection.Select, null);

            if (isRoot)
            {
                ParameterExpression parExp;
                LambdaExpression gator;

                if (isAsync)
                {
                    var taskEnumType = ReflectionCache.GetMember("TaskEnumerableType", valueType, k => typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(k)));
                    parExp = Expression.Parameter(taskEnumType, "p");
                    gator = Expression.Lambda(Expression.Call(typeof(Extensions), nameof(Extensions.SingleOrDefaultCoreAsnyc), new Type[] { valueType }, parExp), parExp);
                }
                else
                {
                    var enumType = ReflectionCache.GetMember("EnumerableType", valueType, k => typeof(IEnumerable<>).MakeGenericType(k));
                    parExp = Expression.Parameter(enumType, "p");
                    gator = Expression.Lambda(Expression.Call(typeof(Enumerable), nameof(Enumerable.SingleOrDefault), new[] { valueType }, parExp), parExp);
                }

                return new ProjectionExpression(select, new ColumnExpression(valueType, alias, string.Empty, null), gator, isAsync, _isNoTracking);
            }

            var subquery = new ScalarExpression(valueType, select);

            // if we can find the corresponding group-info we can build a special AggregateSubquery node that will enable us to
            // optimize the aggregate expression later using AggregateRewriter
            if (!argumentWasPredicate && _groupByMap.TryGetValue(projection, out GroupByInfo info))
            {
                // use the element expression from the group-by info to rebind the argument so the resulting expression is one that
                // would be legal to add to the columns in the select expression that has the corresponding group-by clause.
                if (argument != null)
                {
                    _expMaps[argument.Parameters[0]] = info.Element;
                    argExpr = Visit(argument.Body);
                }
                else if (!hasPredicateArg)
                {
                    argExpr = info.Element;
                }

                aggExpr = new AggregateExpression(valueType, aggType, argExpr, isDistinct);

                // check for easy to optimize case.  If the projection that our aggregate is based on is really the 'group' argument from
                // the query.GroupBy(xxx, (key, group) => yyy) method then whatever expression we return here will automatically
                // become part of the select expression that has the group-by clause, so just return the simple aggregate expression.
                if (projection == m_currentGroupElement)
                {
                    return aggExpr;
                }

                return new AggregateSubqueryExpression(info.Alias, aggExpr, subquery);
            }

            return subquery;
        }

        private Expression BindDistinct(Expression source)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null, null, null, true, null, null, null, null, false),
                pc.Projector, _isQueryAsync, _isNoTracking
                );
        }

        private Expression BindTake(Expression source, Expression take)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            take = Visit(take);
            var select = projection.Select;
            return projection.Update(select.Update(select.From, select.Where, select.OrderBy, select.GroupBy, select.Skip, take, select.Segment, select.IsDistinct, select.Columns, select.Having, select.IsReverse),
                projection.Projector, projection.Aggregator);
            /*
            var alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null, null, null, false, null, take),
                pc.Projector
                );
             */
        }

        private Expression BindCast(Expression source, Type targetElementType)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            var elementType = GetTrueUnderlyingType(projection.Projector);
            if (!targetElementType.IsAssignableFrom(elementType))
            {
                throw new InvalidOperationException(string.Format(string.Empty, elementType, targetElementType));
            }

            return projection;
        }

        private Expression BindSkip(Expression source, Expression skip)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            skip = Visit(skip);
            var select = projection.Select;
            return projection.Update(select.Update(select.From, select.Where, select.OrderBy, select.GroupBy, skip, select.Take, select.Segment, select.IsDistinct, select.Columns, select.Having, select.IsReverse),
                projection.Projector, projection.Aggregator);
            /*
            var alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null, null, null, false, skip, null),
                pc.Projector
                );
             */
        }

        private Expression BindSegment(Expression source, Expression segExp)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            segExp = Visit(segExp);

            if (segExp != null && segExp is ConstantExpression consExp)
            {
                var segment = consExp.Value as IDataSegment;
                var exp = new SegmentExpression(segment);
                var select = projection.Select;
                return projection.Update(select.Update(select.From, select.Where, select.OrderBy, select.GroupBy, null, null, exp, select.IsDistinct, select.Columns, select.Having, select.IsReverse),
                    projection.Projector, projection.Aggregator);
            }

            return projection;
        }

        private Expression BindFirst(Expression source, LambdaExpression predicate, string kind, bool isRoot, bool isAsync = false)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            Expression where = null;
            if (predicate != null)
            {
                _expMaps[predicate.Parameters[0]] = projection.Projector;
                where = Visit(predicate.Body);
            }

            var isFirst = kind.StartsWith("First");
            var isLast = kind.StartsWith("Last");

            Expression logicalDeleteExp = (isFirst || isLast) ? Expression.Constant(1) : null;

            if (logicalDeleteExp != null || where != null)
            {
                var alias = GetNextAlias();
                var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
                projection = new ProjectionExpression(
                    new SelectExpression(alias, pc.Columns, projection.Select, where, null, null, false, null, logicalDeleteExp, null, null, isLast),
                    pc.Projector, isAsync, _isNoTracking
                    );
            }

            if (isRoot)
            {
                var elementType = projection.Projector.Type;
                LambdaExpression gator;

                if (isAsync)
                {
                    var taskEnumType = ReflectionCache.GetMember("TaskEnumerableType", elementType, k => typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(k)));
                    var parExp = Expression.Parameter(taskEnumType, "p");
                    gator = Expression.Lambda(Expression.Call(typeof(Extensions), nameof(Extensions.FirstOrDefaultCoreAsnyc), new Type[] { elementType }, parExp), parExp);
                }
                else
                {
                    var enumType = ReflectionCache.GetMember("EnumerableType", elementType, k => typeof(IEnumerable<>).MakeGenericType(k));
                    var parExp = Expression.Parameter(enumType, "p");
                    gator = Expression.Lambda(Expression.Call(typeof(Enumerable), kind, new Type[] { elementType }, parExp), parExp);
                }

                return new ProjectionExpression(projection.Select, projection.Projector, gator, isAsync, _isNoTracking);
            }

            return projection;
        }

        private Expression BindAnyAll(Expression source, MethodInfo method, LambdaExpression predicate, bool isRoot, bool isAsync)
        {
            var isAll = method.Name.StartsWith(nameof(Queryable.All));
            if (source is ConstantExpression consExp && !IsQueryable(consExp))
            {
                Expression where = null;
                var array = (IEnumerable)consExp.Value;

                var parType = predicate.Parameters[0].Type;

                foreach (var value in array)
                {
                    Expression expr = Expression.Invoke(predicate, Expression.Constant(value, parType));
                    if (where == null)
                    {
                        where = expr;
                    }
                    else if (isAll)
                    {
                        where = Expression.And(where, expr);
                    }
                    else
                    {
                        where = Expression.Or(where, expr);
                    }
                }

                return Visit(where);
            }

            if (isAll && predicate != null)
            {
                predicate = Expression.Lambda(Expression.Not(predicate.Body), predicate.Parameters.ToArray());
            }

            if (predicate != null)
            {
                source = Expression.Call(typeof(Queryable), nameof(Queryable.Where), method.GetGenericArguments(), source, predicate);
            }

            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            Expression result = new ExistsExpression(projection.Select);
            if (isAll)
            {
                result = Expression.Not(result);
            }

            if (isRoot)
            {
                if (_transContext.SyntaxProvider.SupportSubqueryInSelectWithoutFrom)
                {
                    return GetSingletonSequence(result, isAsync);
                }
                else
                {
                    // use count aggregate instead of exists
                    var newSelect = projection.Select.SetColumns(
                        new[] { new ColumnDeclaration("v", new AggregateExpression(typeof(int), AggregateType.Count, null, false)) }
                        );

                    var colx = new ColumnExpression(typeof(int), newSelect.Alias, "v", null);
                    var exp = isAll
                        ? colx.Equal(Expression.Constant(0))
                        : colx.GreaterThan(Expression.Constant(0));

                    return new ProjectionExpression(
                        newSelect, exp, QueryUtility.GetAggregator(typeof(bool), typeof(IEnumerable<bool>)), projection.IsAsync, _isNoTracking
                        );
                }
            }

            return result;
        }

        private Expression GetSingletonSequence(Expression expr, bool isAsync)
        {
            LambdaExpression gator = null;
            if (isAsync)
            {
                var taskEnumType = ReflectionCache.GetMember("TaskEnumerableType", expr.Type, k => typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(k)));
                var parExp = Expression.Parameter(taskEnumType, "p");
                gator = Expression.Lambda(Expression.Call(typeof(Extensions), nameof(Extensions.SingleOrDefaultCoreAsnyc), new[] { expr.Type }, parExp), parExp);
            }
            else
            {
                var enumType = ReflectionCache.GetMember("EnumerableType", expr.Type, k => typeof(IEnumerable<>).MakeGenericType(k));
                var parExp = Expression.Parameter(enumType, "p");
                gator = Expression.Lambda(Expression.Call(typeof(Enumerable), nameof(Enumerable.SingleOrDefault), new[] { expr.Type }, parExp), parExp);
            }

            var alias = GetNextAlias();
            var select = new SelectExpression(alias, new[] { new ColumnDeclaration("v", expr) }, null, null);
            return new ProjectionExpression(select, new ColumnExpression(expr.Type, alias, "v", null), gator, isAsync, _isNoTracking);
        }

        private Expression BindContains(Expression source, Expression match, bool isRoot)
        {
            if (source is ConstantExpression consExp && !IsQueryable(consExp))
            {
                var elementType = ReflectionCache.GetMember("EnumerableElementType", consExp.Type, k => k.GetEnumerableElementType());
                var array = (IEnumerable)consExp.Value;
                if (array.IsNullOrEmpty())
                {
                    return Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(1), Expression.Constant(0));
                }

                if (typeof(IEntity).IsAssignableFrom(elementType))
                {
                    return BindEntityContains(array, match, elementType);
                }

                var values = (from object s in array select (Expression)Expression.Constant(s.ToType(match.Type), match.Type)).ToList();
                match = Visit(match);
                return new InExpression(match, values);
            }
            else if (isRoot)
            {
                var elementType = ReflectionCache.GetMember("EnumerableElementType", source.Type, k => k.GetEnumerableElementType());
                var parExp = Expression.Parameter(elementType, "x");
                var predicate = Expression.Lambda(parExp.Equal(match), parExp);
                var exp = Expression.Call(typeof(Queryable), nameof(Queryable.Any), new Type[] { parExp.Type }, source, predicate);
                _root = exp;
                return Visit(exp);
            }

            if (source is MethodCallExpression mcallExp && mcallExp.Arguments[0] is ConstantExpression consExp1)
            {
                if (!IsQueryable(consExp1))
                {
                    //todo
                }
            }

            //属性是一个集合的情况
            if (source is MemberExpression mbrExp && source.Type.IsArray ||
                (source.Type.IsGenericParameter &&
                    (source.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || source.Type.GetGenericTypeDefinition() == typeof(List<>))))
            {
                var elementType = ReflectionCache.GetMember("EnumerableElementType", source.Type, k => k.GetEnumerableElementType());
                return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { elementType }, Visit(source), Visit(match));
            }

            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            if ((DbExpressionType)projection.NodeType != DbExpressionType.Projection)
            {
                return null;
            }

            match = Visit(match);

            var result = new InExpression(match, projection.Select);
            return isRoot ? GetSingletonSequence(result, false) : result;
        }

        /// <summary>
        /// 处理实体集的Contains方法
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="parExp"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private Expression BindEntityContains(IEnumerable enumerable, Expression parExp, Type entityType)
        {
            //使用所有主键进行匹配
            var properties = PropertyUnity.GetPrimaryProperties(entityType);

            if (!properties.Any())
            {
                throw new NotSupportedException(SR.GetString(SRKind.NotDefinedPrimaryKey));
            }

            var exp = properties.Select(p =>
                {
                    var values = (from IEntity s in enumerable select (Expression)Expression.Constant(s.GetValue(p))).ToList();
                    var matchExp = Visit(Expression.Property(parExp, p.Name));
                    return (Expression)new InExpression(matchExp, values);
                }).Aggregate(Expression.And);

            if (exp == null)
            {
                throw new Exception(SR.GetString(SRKind.NotDefinedPrimaryKey));
            }

            return exp;
        }

        /// <summary>
        /// 绑定 Delete 命令。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="logicalDeleteExp"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        private Expression BindDelete(Expression source, LambdaExpression predicate, Expression logicalDeleteExp, bool isAsync)
        {
            _ = VisitSequence(source);

            var entityType = predicate.Parameters[0].Type;
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            var logicalDelete = (bool)(PropertyValue)((ConstantExpression)logicalDeleteExp).Value;
            predicate = (LambdaExpression)Visit(predicate);

            if (metadata.DeleteProperty != null && logicalDelete)
            {
                return Visit(QueryUtility.GetLogicalDeleteExpression(_transContext, metadata, predicate, isAsync));
            }

            return Visit(QueryUtility.GetDeleteExpression(_transContext, metadata, predicate, true, isAsync));
        }

        /// <summary>
        /// 绑定 Delete 命令。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parExp"></param>
        /// <param name="logicalDeleteExp"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        private Expression BindDelete(Expression source, ParameterExpression parExp, Expression logicalDeleteExp, bool isAsync)
        {
            _ = VisitSequence(source);

            var metadata = EntityMetadataUnity.GetEntityMetadata(parExp.Type);
            var logicalDelete = (bool)((ConstantExpression)logicalDeleteExp).Value;
            var predicate = QueryUtility.GetPrimaryKeyExpression(_transContext, parExp);

            if (metadata.DeleteProperty != null && logicalDelete)
            {
                return Visit(QueryUtility.GetLogicalDeleteExpression(_transContext, metadata, predicate, isAsync));
            }

            return Visit(QueryUtility.GetDeleteExpression(_transContext, metadata, predicate, false, isAsync));
        }

        /// <summary>
        /// 绑定 Update 命令。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="instance"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private Expression BindUpdate(Expression source, Expression instance, LambdaExpression predicate, bool isAsync)
        {
            _ = VisitSequence(source);
            if (instance is ParameterExpression)
            {
                if (predicate == null)
                {
                    predicate = QueryUtility.GetPrimaryKeyExpression(_transContext, (ParameterExpression)instance);
                }
                else
                {
                    predicate = (LambdaExpression)PredicateReplacer.Replace(_transContext, predicate, predicate.Parameters[0]);
                }
            }
            else if (instance is ConstantExpression)
            {
                predicate = BindConcurrencyLockingExpression((ConstantExpression)instance, predicate);
            }

            predicate = (LambdaExpression)Visit(predicate);

            return Visit(QueryUtility.GetUpdateExpression(_transContext, instance, predicate, isAsync));
        }

        /// <summary>
        /// 加入并发控制条件生成新的表达式。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private LambdaExpression BindConcurrencyLockingExpression(ConstantExpression instance, LambdaExpression predicate)
        {
            var entity = instance.Value as IEntity;
            var parExp = predicate.Parameters[0];
            var body = predicate.Body;
            var properties = EntityMetadataUnity.GetEntityMetadata(entity.EntityType).ConcurrencyProperties;

            foreach (var property in properties)
            {
                var value = entity.GetOldValue(property.Name);
                var cvtValue = Expression.Convert(Expression.Constant(value), property.Type);
                var equalExp = Expression.Equal(Expression.MakeMemberAccess(parExp, property.Info.ReflectionInfo), cvtValue);
                body = Expression.And(body, equalExp);
            }

            return Expression.Lambda(body, parExp);
        }

        private Expression BindInsert(Expression source, Expression instance, bool isAsync)
        {
            _ = VisitSequence(source);

            return Visit(QueryUtility.GetInsertExpression(_transContext, instance, isAsync));
        }

        private Expression BindBatch(Expression source, Expression instances, LambdaExpression operation, Expression batchOptExp, bool isAsync)
        {
            _ = VisitSequence(source);

            BatchOperateOptions options = null;

            //取参数
            if (batchOptExp != null && batchOptExp is ConstantExpression conOpt && conOpt.Value != null)
            {
                options = conOpt.Value as BatchOperateOptions;
            }

            var properties = QueryUtility.GetModifiedProperties(instances, options);

            //在序列中查找被修改的属性列表
            _transContext.TemporaryBag = properties;

            var op = (LambdaExpression)Visit(operation);

            _transContext.TemporaryBag = null;

            var items = (ConstantExpression)Visit(instances);
            return new BatchCommandExpression(items, op, isAsync, properties);
        }

        public Expression BindExtend(Expression source, LambdaExpression selector)
        {
            var arguments = new List<Expression>();
            var members = new List<MemberInfo>();

            if (selector != null)
            {
                var expression = Visit(selector.Body);
                if (expression.NodeType == ExpressionType.Convert)
                {
                    expression = (expression as UnaryExpression).Operand;
                }

                if (expression.NodeType == ExpressionType.New)
                {
                    var newExp = expression as NewExpression;
                    arguments = newExp.Arguments.ToList();
                    members = newExp.Members.ToList();
                }
                else if (expression.NodeType == ExpressionType.MemberInit)
                {
                    var initExp = expression as MemberInitExpression;
                    foreach (MemberAssignment m in initExp.Bindings.Where(s => s.BindingType == MemberBindingType.Assignment))
                    {
                        arguments.Add(m.Expression);
                        members.Add(m.Member);
                    }
                }
                else if (expression.NodeType == ExpressionType.Constant)
                {
                    var obj = (expression as ConstantExpression).Value;
                    foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var value = property.FastGetValue(obj);
                        if (value != null)
                        {
                            arguments.Add(Expression.Constant(value, property.PropertyType));
                            members.Add(property);
                        }
                    }
                }
            }

            var sourceType = ParameterTypeFinder.Find(source);
            foreach (var property in PropertyUnity.GetPersistentProperties(sourceType))
            {
                var columnExp = new ColumnExpression(property.Type, _alias, property.Name, property.Info);
                arguments.Add(columnExp);
                members.Add(property.Info.ReflectionInfo);
            }

            var keyPairArray = new Expression[members.Count];
            var constructor = typeof(KeyValuePair<string, object>).GetConstructors()[0];
            for (var i = 0; i < members.Count; i++)
            {
                keyPairArray[i] = Expression.New(constructor, Expression.Constant(members[i].Name), Expression.Convert(arguments[i], typeof(object)));
            }

            var arrayExp = Expression.NewArrayInit(typeof(KeyValuePair<string, object>), keyPairArray);

            return Expression.Convert(arrayExp, typeof(DynamicExpandoObject));
        }

        public Expression BindExtendAs(Type returnType, Expression source, LambdaExpression selector)
        {
            var memberInits = new List<MemberBinding>();

            if (selector != null)
            {
                var arguments = new List<Expression>();
                var members = new List<MemberInfo>();

                var expression = Visit(selector.Body);
                if (expression.NodeType == ExpressionType.Convert)
                {
                    expression = (expression as UnaryExpression).Operand;
                }

                if (expression.NodeType == ExpressionType.New)
                {
                    var newExp = expression as NewExpression;
                    for (var i = 0; i < newExp.Arguments.Count; i++)
                    {
                        var prop = returnType.GetProperty(newExp.Members[i].Name);
                        if (prop != null && prop.CanWrite)
                        {
                            if (newExp.Arguments[i].Type != prop.PropertyType)
                            {
                                throw new ArgumentException(newExp.Members[i].Name);
                            }

                            memberInits.Add(Expression.Bind(prop, newExp.Arguments[i]));
                        }
                    }
                }
                else if (expression.NodeType == ExpressionType.MemberInit)
                {
                    var mbrInit = expression as MemberInitExpression;
                    foreach (var m in mbrInit.Bindings.Where(s => s.BindingType == MemberBindingType.Assignment))
                    {
                        memberInits.Add(m as MemberBinding);
                    }
                }
                else if (expression.NodeType == ExpressionType.Constant)
                {
                    var obj = (expression as ConstantExpression).Value;
                    foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var value = property.FastGetValue(obj);
                        if (value != null)
                        {
                            memberInits.Add(Expression.Bind(property, Expression.Constant(value, property.PropertyType)));
                        }
                    }
                }
            }

            foreach (var property in PropertyUnity.GetPersistentProperties(returnType))
            {
                var prop = property.Info.ReflectionInfo;

                if (!prop.CanWrite)
                {
                    continue;
                }

                var columnExp = new ColumnExpression(property.Type, _alias, property.Name, property.Info);
                memberInits.Add(Expression.Bind(prop, columnExp));
            }

            return Expression.MemberInit(Expression.New(returnType), memberInits);
        }

        public Expression BindTo(Type returnType, Expression source)
        {
            var memberInits = new List<MemberBinding>();
            foreach (var property in PropertyUnity.GetPersistentProperties(returnType))
            {
                var prop = property.Info.ReflectionInfo;

                if (!prop.CanWrite)
                {
                    continue;
                }

                var columnExp = new ColumnExpression(property.Type, _alias, property.Name, property.Info);
                memberInits.Add(Expression.Bind(prop, columnExp));
            }

            return Expression.MemberInit(Expression.New(returnType), memberInits);
        }

        /// <summary>
        /// 判断是否为 IQueryable
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool IsQueryable(Expression expression)
        {
            var elementType = ReflectionCache.GetMember("EnumerableElementType", expression.Type, k => k.GetEnumerableElementType());
            if (elementType != null)
            {
                var queryableType = ReflectionCache.GetMember("QueryableType1", elementType, k => typeof(IQueryable<>).MakeGenericType(k));
                return queryableType.IsAssignableFrom(expression.Type);
            }

            return false;
        }

        private NewExpression GetNewExpression(Expression expression)
        {
            // ignore converions 
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression as NewExpression;
        }

        internal static Expression BindMember(Expression source, MemberInfo member)
        {
            switch (source.NodeType)
            {
                case (ExpressionType)DbExpressionType.Entity:
                    var ex = (EntityExpression)source;
                    var result = BindMember(ex.Expression, member);
                    if (result is MemberExpression mex &&
                        mex.Expression == ex.Expression &&
                        mex.Member == member)
                    {
                        return Expression.MakeMemberAccess(source, member);
                    }

                    return result;

                case ExpressionType.MemberInit:
                    var min = (MemberInitExpression)source;
                    for (int i = 0, n = min.Bindings.Count; i < n; i++)
                    {
                        if (min.Bindings[i] is MemberAssignment assign &&
                            MembersMatch(assign.Member, member))
                        {
                            return assign.Expression;
                        }
                    }
                    break;

                case ExpressionType.New:
                    var nex = (NewExpression)source;
                    if (nex.Members != null)
                    {
                        for (int i = 0, n = nex.Members.Count; i < n; i++)
                        {
                            if (MembersMatch(nex.Members[i], member))
                            {
                                return nex.Arguments[i];
                            }
                        }
                    }
                    else if (nex.Type.IsGenericType &&
                        nex.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
                    {
                        if (member.Name == "Key")
                        {
                            return nex.Arguments[0];
                        }
                    }
                    break;

                case (ExpressionType)DbExpressionType.Projection:
                    var proj = (ProjectionExpression)source;
                    var newProjector = BindMember(proj.Projector, member);
                    var mt = member.GetMemberType();
                    return new ProjectionExpression(proj.Select, newProjector, QueryUtility.GetAggregator(mt, typeof(IEnumerable<>).MakeGenericType(mt)), proj.IsAsync, proj.IsNoTracking);

                case (ExpressionType)DbExpressionType.OuterJoined:
                    var oj = (OuterJoinedExpression)source;
                    var em = BindMember(oj.Expression, member);
                    if (em is ColumnExpression)
                    {
                        return em;
                    }
                    return new OuterJoinedExpression(oj.Test, em);

                case ExpressionType.Conditional:
                    var cex = (ConditionalExpression)source;
                    return Expression.Condition(cex.Test, BindMember(cex.IfTrue, member), BindMember(cex.IfFalse, member));

                case ExpressionType.Constant:
                    var con = (ConstantExpression)source;
                    var memberType = member.GetMemberType();

                    if (con.Value == null)
                    {
                        return Expression.Constant(memberType.GetDefaultValue());
                    }

                    var value = member.GetMemberValue(con.Value);
                    return Expression.Constant(value, memberType);

                case ExpressionType.Convert:
                    var conv = (UnaryExpression)source;
                    return BindMember(conv.Operand, member);
            }

            return Expression.MakeMemberAccess(source, member);
        }

        /// <summary>
        /// 匹配成员。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool MembersMatch(MemberInfo a, MemberInfo b)
        {
            if (a == b)
            {
                return true;
            }
            if (a is MethodInfo && b is PropertyInfo)
            {
                return a == ((PropertyInfo)b).GetGetMethod();
            }
            if (a is PropertyInfo && b is MethodInfo)
            {
                return ((PropertyInfo)a).GetGetMethod() == b;
            }
            //使用接口实现查询
            else if (b.DeclaringType.IsAssignableFrom(a.DeclaringType))
            {
                if (a.Name == b.Name)
                {
                    return true;
                }

                if (a.IsDefined<InterfaceMemberMappingAttribute>())
                {
                    var attr = a.GetCustomAttributes<InterfaceMemberMappingAttribute>().FirstOrDefault();
                    if (attr != null)
                    {
                        return attr.Type == b.DeclaringType && attr.Name == a.Name;
                    }
                }
            }

            return false;
        }

        private class PredicateReplacer : Common.Linq.Expressions.ExpressionVisitor
        {
            private readonly ParameterExpression _parExp;
            private readonly TableExpression _table;
            private readonly TranslateContext _transContext;

            protected PredicateReplacer(TranslateContext transContext, ParameterExpression parExp)
            {
                _transContext = transContext;
                _parExp = parExp;

                var metadata = EntityMetadataUnity.GetEntityMetadata(parExp.Type);
                _table = new TableExpression(new TableAlias(), metadata.TableName, parExp.Type);
            }

            public static Expression Replace(TranslateContext transContext, Expression expression, ParameterExpression parExp)
            {
                return new PredicateReplacer(transContext, parExp).Visit(expression);
            }

            protected override Expression VisitMember(MemberExpression memberExp)
            {
                if (memberExp.Expression == _parExp)
                {
                    var property = PropertyUnity.GetProperty(memberExp.Member.DeclaringType, memberExp.Member.Name);
                    if (property == null)
                    {
                        return memberExp;
                    }

                    return QueryUtility.GetMemberExpression(_transContext, _table, property);
                }

                return memberExp;
            }
        }

        private class ParameterTypeFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private Type _parameterType;

            public static Type Find(Expression expression)
            {
                var finder = new ParameterTypeFinder();
                finder.Visit(expression);
                return finder._parameterType;
            }

            protected override Expression VisitParameter(ParameterExpression parExp)
            {
                _parameterType = parExp.Type;
                return parExp;
            }
        }
    }
}