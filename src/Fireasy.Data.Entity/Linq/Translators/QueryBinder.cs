// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Dynamic;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
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
        private readonly Dictionary<ParameterExpression, Expression> expMaps;
        private readonly Dictionary<Expression, GroupByInfo> groupByMap;
        private Expression root;
        private ISyntaxProvider syntax;
        private List<OrderExpression> thenBys;
        private Expression batchSource;
        private bool isNoTracking = false;
        private bool isQueryAsync = false;

        private QueryBinder(Expression root)
        {
            expMaps = new Dictionary<ParameterExpression, Expression>();
            groupByMap = new Dictionary<Expression, GroupByInfo>();
            this.root = root;
        }

        /// <summary>
        /// 将 Linq 查询表达式绑定为 ELinq 表示。
        /// </summary>
        /// <param name="expression">Linq 表达式。</param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static Expression Bind(Expression expression, ISyntaxProvider syntax)
        {
            return new QueryBinder(expression) { syntax = syntax }.Visit(expression);
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
                return binder.Bind(new MethodCallBindContext(this, node, syntax));
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
                    return BindContains(node.Object, node.Arguments[0], node == root);
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
                        batchSource = node.Arguments[0];
                        return BindBatch(node.Arguments[0], node.Arguments[1], GetLambda(node.Arguments[2]), isAsync);
                    case nameof(Extensions.Extend):
                        return BindExtend(node.Arguments[0], GetLambda(node.Arguments[1]));
                    case nameof(Extensions.ExtendAs):
                        return BindExtendAs(node.Type, node.Arguments[0], GetLambda(node.Arguments[1]));
                    case nameof(Extensions.AsNoTracking):
                        isNoTracking = true;
                        return Visit(node.Arguments[0]);
                    case nameof(Extensions.CacheParsing):
                    case nameof(Extensions.CacheExecution):
                        return Visit(node.Arguments[0]);
                    case nameof(Extensions.FirstOrDefaultAsync):
                    case nameof(Extensions.LastOrDefaultAsync):
                        if (node.Arguments.Count == 2)
                        {
                            return BindFirst(node.Arguments[0], null, node.Method.Name.Replace("Async", string.Empty), node == this.root, true);
                        }
                        else if (node.Arguments.Count == 3)
                        {
                            return BindFirst(node.Arguments[0], GetLambda(node.Arguments[1]), node.Method.Name.Replace("Async", string.Empty), node == this.root, true);
                        }
                        break;
                    case nameof(Extensions.AnyAsync):
                    case nameof(Extensions.AllAsync):
                        isQueryAsync = true;
                        return BindAnyAll(node.Arguments[0], node.Method, node.Arguments.Count == 3 ? GetLambda(node.Arguments[1]) : null, node == root);
                    case nameof(Extensions.ToListAsync):
                        isQueryAsync = true;
                        return Visit(node.Arguments[0]);
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
                            return BindDelete(batchSource, predicate1, node.Arguments[1], isAsync);
                        }
                        else
                        {
                            return BindDelete(batchSource, (ParameterExpression)node.Arguments[0], node.Arguments[1], isAsync);
                        }
                    case nameof(IRepository.Update):
                    case nameof(IRepository.UpdateAsync):
                        var predicate2 = node.Arguments.Count > 1 ? GetLambda(node.Arguments[1]) : null;
                        return BindUpdate(batchSource, node.Arguments[0], predicate2, isAsync);
                    case nameof(IRepository.Insert):
                    case nameof(IRepository.InsertAsync):
                        return BindInsert(batchSource, node.Arguments[0], isAsync);
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
                var rowType = c.Type.GetEnumerableElementType();
                if (typeof(IEntity).IsAssignableFrom(rowType) &&
                    q.Expression.NodeType == ExpressionType.Constant)
                {
                    return VisitSequence(QueryUtility.GetTableQuery(EntityMetadataUnity.GetEntityMetadata(rowType), isNoTracking, isQueryAsync));
                }
                else if (q.Expression.NodeType == ExpressionType.Constant)
                {
                    // assume this is also a table via some other implementation of IQueryable
                    return VisitSequence(QueryUtility.GetTableQuery(EntityMetadataUnity.GetEntityMetadata(q.ElementType), isNoTracking, isQueryAsync));
                }
                else
                {
                    var translator = TranslateScope.Current.Translator;
                    var pev = Common.Linq.Expressions.PartialEvaluator.Eval(q.Expression, translator.CanBeEvaluatedLocally);
                    return this.Visit(pev);
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
            Expression e;
            return expMaps.TryGetValue(p, out e) ? e : p;
        }

        /// <summary>
        /// 访问 <see cref="InvocationExpression"/>。
        /// </summary>
        /// <param name="iv">要访问的表达式。</param>
        /// <returns></returns>
        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            var lambda = iv.Expression as LambdaExpression;
            if (lambda != null)
            {
                for (int i = 0, n = lambda.Parameters.Count; i < n; i++)
                {
                    expMaps[lambda.Parameters[i]] = iv.Arguments[i];
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
                && !expMaps.ContainsKey((ParameterExpression)m.Expression)
                && IsQueryable(m))
            {
                return VisitSequence(QueryUtility.GetTableQuery(EntityMetadataUnity.GetEntityMetadata(m.Type.GetEnumerableElementType()), isNoTracking, isQueryAsync));
            }

            if (m.Member.DeclaringType.IsNullableType() && m.Member.Name == nameof(Nullable<int>.HasValue))
            {
                return Visit(Expression.NotEqual(m.Expression, Expression.Constant(null)));
            }

            var source = Visit(m.Expression);
            if (IsAggregate(m.Member) && IsRemoteQuery(source))
            {
                return BindAggregate(m.Expression, m.Member, null, m == this.root);
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
            var list = new List<ColumnAssignment>();
            foreach (var argument in update.Assignments)
            {
                list.Add(new ColumnAssignment(argument.Column, Visit(argument.Expression)));
            }

            return update.Update(update.Table, update.Where, list);
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
                    var bound = this.BindRelationshipProperty((MemberExpression)expr);
                    if (bound.NodeType != ExpressionType.MemberAccess)
                    {
                        return this.ConvertToSequence(bound);
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
                    var queryable = constExp.Value as IQueryable;
                    if (queryable != null)
                    {
                        return (ProjectionExpression)Bind(queryable.Expression, syntax);
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
            var ex = mex.Expression as EntityExpression;
            if (ex != null)
            {
                var property = PropertyUnity.GetProperty(ex.Type, mex.Member.Name);
                if (property is RelationProperty)
                {
                    return QueryUtility.GetMemberExpression(mex.Expression, property);
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
                var projection = result as ProjectionExpression;
                if (projection != null &&
                    projection.Aggregator == null &&
                    !expectedType.IsAssignableFrom(projection.Type))
                {
                    var aggregator = QueryUtility.GetAggregator(expectedType, projection.Type);
                    if (aggregator != null)
                    {
                        return new ProjectionExpression(projection.Select, projection.Projector, aggregator, isQueryAsync);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 绑定 Where 子句。
        /// </summary>
        /// <param name="resultType"></param>
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

            expMaps[predicate.Parameters[0]] = projection.Projector;
            var where = Visit(predicate.Body);
            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, where),
                pc.Projector, isQueryAsync
                );
        }

        private Expression BindReverse(Expression source)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            var alias = this.GetNextAlias();
            ProjectedColumns pc = this.ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null).Reverse(true),
                pc.Projector, isQueryAsync
                );
        }


        private TableAlias __alias;
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

            expMaps[selector.Parameters[0]] = projection.Projector;
            __alias = projection.Select.Alias;
            var expression = Visit(selector.Body);
            __alias = null;
            var alias = GetNextAlias();
            var pc = ProjectColumns(expression, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null),
                pc.Projector, isQueryAsync
                );
        }

        private Expression BindSelectMany(Type resultType, Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            expMaps[collectionSelector.Parameters[0]] = projection.Projector;

            var collection = collectionSelector.Body;

            bool defaultIfEmpty = false;
            if (collection is MethodCallExpression mcs && mcs.Method.Name == nameof(Queryable.DefaultIfEmpty) && mcs.Arguments.Count == 1 &&
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
                expMaps[resultSelector.Parameters[0]] = projection.Projector;
                expMaps[resultSelector.Parameters[1]] = collectionProjection.Projector;
                var result = Visit(resultSelector.Body);
                pc = ProjectColumns(result, alias, projection.Select.Alias, collectionProjection.Select.Alias);
            }
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, join, null),
                pc.Projector, isQueryAsync
                );
        }

        private Expression BindJoin(Type resultType, Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector)
        {
            var joinType = GetJoinType(ref outerSource, ref innerSource);
            var outerProjection = VisitSequence(outerSource);
            var innerProjection = VisitSequence(innerSource);

            expMaps[outerKey.Parameters[0]] = outerProjection.Projector;
            var outerKeyExpr = GroupKeyReplacer.Replace(Visit(outerKey.Body));
            expMaps[innerKey.Parameters[0]] = innerProjection.Projector;
            var innerKeyExpr = GroupKeyReplacer.Replace(Visit(innerKey.Body));
            expMaps[resultSelector.Parameters[0]] = outerProjection.Projector;
            expMaps[resultSelector.Parameters[1]] = innerProjection.Projector;
            __alias = outerProjection.Select.Alias;
            var resultExpr = Visit(resultSelector.Body);
            __alias = null;
            var join = new JoinExpression(joinType, outerProjection.Select, innerProjection.Select, outerKeyExpr.Equal(innerKeyExpr));
            var alias = GetNextAlias();
            var pc = ProjectColumns(resultExpr, alias, outerProjection.Select.Alias, innerProjection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, join, null),
                pc.Projector, isQueryAsync
                );
        }

        private Expression ReplaceGroupKey(Expression expression)
        {
            return expression;
        }

        private Expression BindIntersect(Expression outerQueryable, Expression innerQueryable, bool negate)
        {
            // SELECT * FROM outer WHERE EXISTS(SELECT * FROM inner WHERE inner = outer))
            ProjectionExpression outerProjection = this.VisitSequence(outerQueryable);
            ProjectionExpression innerProjection = this.VisitSequence(innerQueryable);

            Expression exists = new ExistsExpression(
                new SelectExpression(new TableAlias(), null, innerProjection.Select, innerProjection.Projector.Equal(outerProjection.Projector))
                );
            if (negate)
                exists = Expression.Not(exists);
            var alias = this.GetNextAlias();
            ProjectedColumns pc = this.ProjectColumns(outerProjection.Projector, alias, outerProjection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, outerProjection.Select, exists),
                pc.Projector, outerProjection.Aggregator, isQueryAsync
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
            var myThenBys = thenBys;
            thenBys = null;
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            expMaps[orderSelector.Parameters[0]] = projection.Projector;
            var orderings = GetOrderExpressions(orderType, orderSelector.Body);

            if (myThenBys != null)
            {
                for (var i = myThenBys.Count - 1; i >= 0; i--)
                {
                    var tb = myThenBys[i];
                    var lambda = (LambdaExpression)tb.Expression;
                    expMaps[lambda.Parameters[0]] = projection.Projector;
                    orderings.AddRange(GetOrderExpressions(tb.OrderType, lambda.Body));
                }
            }

            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null, orderings.AsReadOnly(), null, null),
                pc.Projector, isQueryAsync
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
                var orderings = new List<OrderExpression>();
                var newExp = expression as NewExpression;
                foreach (var ex in newExp.Arguments)
                {
                    orderings.Add(new OrderExpression(orderType, Visit(ex)));
                }

                return orderings;
            }
            else
            {
                return new List<OrderExpression> { new OrderExpression(orderType, Visit(expression)) };
            }
        }

        private Expression BindThenBy(Expression source, LambdaExpression orderSelector, OrderType orderType)
        {
            if (thenBys == null)
            {
                thenBys = new List<OrderExpression>();
            }

            thenBys.Add(new OrderExpression(orderType, orderSelector));
            return Visit(source);
        }

        private Expression BindGroupBy(Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            expMaps[keySelector.Parameters[0]] = projection.Projector;
            var keyExpr = Visit(keySelector.Body);

            var elemExpr = projection.Projector;
            if (elementSelector != null)
            {
                expMaps[elementSelector.Parameters[0]] = projection.Projector;
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
            expMaps[keySelector.Parameters[0]] = subqueryBasis.Projector;
            var subqueryKey = Visit(keySelector.Body);

            // use same projection trick to get group-by expressions based on subquery
            var subqueryKeyPC = ProjectColumns(subqueryKey, subqueryBasis.Select.Alias, subqueryBasis.Select.Alias);
            var subqueryGroupExprs = subqueryKeyPC.Columns.Select(c => c.Expression);
            var subqueryCorrelation = BuildPredicateWithNullsEqual(subqueryGroupExprs, groupExprs);

            // compute element based on duplicated subquery
            var subqueryElemExpr = subqueryBasis.Projector;
            if (elementSelector != null)
            {
                expMaps[elementSelector.Parameters[0]] = subqueryBasis.Projector;
                subqueryElemExpr = Visit(elementSelector.Body);
            }

            // build subquery that projects the desired element
            var elementAlias = GetNextAlias();
            var elementPC = ProjectColumns(subqueryElemExpr, elementAlias, subqueryBasis.Select.Alias);
            var elementSubquery =
                new ProjectionExpression(
                    new SelectExpression(elementAlias, elementPC.Columns, subqueryBasis.Select, subqueryCorrelation),
                    elementPC.Projector, isQueryAsync
                    );

            var alias = GetNextAlias();

            // make it possible to tie aggregates back to this group-by
            var info = new GroupByInfo(alias, elemExpr);
            groupByMap.Add(elementSubquery, info);

            Expression resultExpr;
            if (resultSelector != null)
            {
                var saveGroupElement = m_currentGroupElement;
                m_currentGroupElement = elementSubquery;
                // compute result expression based on key & element-subquery
                expMaps[resultSelector.Parameters[0]] = keyProjection.Projector;
                expMaps[resultSelector.Parameters[1]] = elementSubquery;
                resultExpr = Visit(resultSelector.Body);
                m_currentGroupElement = saveGroupElement;
            }
            else
            {
                // result must be IGrouping<K,E>
                resultExpr =
                    Expression.New(
                        typeof(Grouping<,>).MakeGenericType(keyExpr.Type, subqueryElemExpr.Type).GetConstructors()[0],
                        new[] { keyExpr, elementSubquery }
                        );

                resultExpr = Expression.Convert(resultExpr, typeof(IGrouping<,>).MakeGenericType(keyExpr.Type, subqueryElemExpr.Type));
            }

            var pc = ProjectColumns(resultExpr, alias, projection.Select.Alias);

            // make it possible to tie aggregates back to this group-by
            var newResult = this.GetNewExpression(pc.Projector);
            if (newResult != null && newResult.Type.IsGenericType && newResult.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
            {
                Expression projectedElementSubquery = newResult.Arguments[1];
                this.groupByMap.Add(projectedElementSubquery, info);
            }

            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, projection.Select, null, null, groupExprs, null),
                pc.Projector, isQueryAsync
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

            expMaps[outerKey.Parameters[0]] = outerProjection.Projector;
            var predicateLambda = Expression.Lambda(innerKey.Body.Equal(outerKey.Body), innerKey.Parameters[0]);
            var callToWhere = Expression.Call(typeof(Enumerable), "Where", new Type[] { args[1] }, innerSource, predicateLambda);
            var group = this.Visit(callToWhere);

            expMaps[resultSelector.Parameters[0]] = outerProjection.Projector;
            expMaps[resultSelector.Parameters[1]] = group;
            var resultExpr = this.Visit(resultSelector.Body);

            var alias = this.GetNextAlias();
            var pc = this.ProjectColumns(resultExpr, alias, outerProjection.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(alias, pc.Columns, outerProjection.Select, null),
                pc.Projector, isQueryAsync
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
            internal TableAlias Alias { get; private set; }

            internal Expression Element { get; private set; }

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
                    return AggregateType.Count;
                case nameof(Enumerable.Min): return AggregateType.Min;
                case nameof(Enumerable.Max): return AggregateType.Max;
                case nameof(Enumerable.Sum): return AggregateType.Sum;
                case nameof(Enumerable.Average): return AggregateType.Average;
                default: throw new TranslateException(null, new Exception(SR.GetString(SRKind.UnknowAggregateType, name)));
            }
        }

        private bool IsAggregate(MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
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
            var property = member as PropertyInfo;
            if (property != null
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
                return true;
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return IsRemoteQuery(((MemberExpression)expression).Expression);
                case ExpressionType.Call:
                    MethodCallExpression mc = (MethodCallExpression)expression;
                    if (mc.Object != null)
                        return IsRemoteQuery(mc.Object);
                    else if (mc.Arguments.Count > 0)
                        return IsRemoteQuery(mc.Arguments[0]);
                    break;
            }
            return false;
        }

        private Type GetTrueUnderlyingType(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
                expression = ((UnaryExpression)expression).Operand;
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
                    newExp = BindAggregate(node.Arguments[0], node.Method, node.Arguments.Count == 2 ? GetLambda(node.Arguments[1]) : null, node == root);
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
                case nameof(Queryable.Intersect):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindIntersect(node.Arguments[0], node.Arguments[1], node.Method.Name == "Except");
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
                        newExp = BindFirst(node.Arguments[0], null, node.Method.Name, node == this.root);
                    }
                    else if (node.Arguments.Count == 2)
                    {
                        newExp = BindFirst(node.Arguments[0], GetLambda(node.Arguments[1]), node.Method.Name, node == this.root);
                    }
                    break;
                case nameof(Queryable.Any):
                    newExp = BindAnyAll(node.Arguments[0], node.Method, node.Arguments.Count == 2 ? GetLambda(node.Arguments[1]) : null, node == root);
                    break;
                case nameof(Queryable.All):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindAnyAll(node.Arguments[0], node.Method, GetLambda(node.Arguments[1]), node == root);
                    }
                    break;
                case nameof(Queryable.Contains):
                    if (node.Arguments.Count == 2)
                    {
                        newExp = BindContains(node.Arguments[0], node.Arguments[1], node == root);
                    }
                    break;
                case nameof(Queryable.DefaultIfEmpty):
                    newExp = Visit(node.Arguments[0]);
                    break;
            }

            if (newExp == null)
            {
                var arguments = node.Arguments.ToArray();
                for (var i = 1; i < arguments.Length; i++)
                {
                    arguments[i] = Visit(arguments[i]);
                }

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

            return m;
        }

        private IQueryable CreateQuery(Type eleType, Expression exp)
        {
            var persister = (exp as ConstantExpression).Value;
            var provider = (persister as IQueryProviderAware).Provider;
            return (IQueryable)typeof(QuerySet<>).MakeGenericType(eleType).New(provider);
        }

        private Expression BindAggregate(Expression source, MemberInfo member, LambdaExpression argument, bool isRoot)
        {
            var returnType = member.GetMemberType();
            var aggType = GetAggregateType(member.Name);
            var hasPredicateArg = HasPredicateArg(aggType);
            var isDistinct = false;
            var argumentWasPredicate = false;

            // check for distinct
            var mcs = source as MethodCallExpression;
            if (mcs != null && !hasPredicateArg && argument == null)
            {
                if (mcs.Method.Name == nameof(Queryable.Distinct) && mcs.Arguments.Count == 1 &&
                    (mcs.Method.DeclaringType == typeof(Queryable) || mcs.Method.DeclaringType == typeof(Enumerable)) &&
                    syntax.SupportDistinctInAggregates)
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
                expMaps[argument.Parameters[0]] = projection.Projector;
                argExpr = Visit(argument.Body);
            }
            else if (!hasPredicateArg)
            {
                argExpr = projection.Projector;
            }

            var alias = GetNextAlias();
            var aggExpr = new AggregateExpression(returnType, aggType, argExpr, isDistinct);
            var select = new SelectExpression(alias, new[] { new ColumnDeclaration("", aggExpr) }, projection.Select, null);

            if (isRoot)
            {
                var p = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(aggExpr.Type), "p");
                var gator = Expression.Lambda(Expression.Call(typeof(Enumerable), nameof(Enumerable.Single), new[] { returnType }, p), p);
                return new ProjectionExpression(select, new ColumnExpression(returnType, alias, string.Empty, null), gator, isQueryAsync);
            }

            var subquery = new ScalarExpression(returnType, select);

            // if we can find the corresponding group-info we can build a special AggregateSubquery node that will enable us to
            // optimize the aggregate expression later using AggregateRewriter
            GroupByInfo info;
            if (!argumentWasPredicate && groupByMap.TryGetValue(projection, out info))
            {
                // use the element expression from the group-by info to rebind the argument so the resulting expression is one that
                // would be legal to add to the columns in the select expression that has the corresponding group-by clause.
                if (argument != null)
                {
                    expMaps[argument.Parameters[0]] = info.Element;
                    argExpr = Visit(argument.Body);
                }
                else if (!hasPredicateArg)
                {
                    argExpr = info.Element;
                }
                aggExpr = new AggregateExpression(returnType, aggType, argExpr, isDistinct);

                // check for easy to optimize case.  If the projection that our aggregate is based on is really the 'group' argument from
                // the query.GroupBy(xxx, (key, group) => yyy) method then whatever expression we return here will automatically
                // become part of the select expression that has the group-by clause, so just return the simple aggregate expression.
                if (projection == m_currentGroupElement)
                    return aggExpr;

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
                pc.Projector, isQueryAsync
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

            Type elementType = GetTrueUnderlyingType(projection.Projector);
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

        private Expression BindSegment(Expression source, Expression segment)
        {
            var projection = VisitSequence(source);
            if (projection == null)
            {
                return null;
            }

            segment = Visit(segment);
            if (segment != null && segment is ConstantExpression)
            {
                var c = (segment as ConstantExpression).Value as IDataSegment;
                var exp = new SegmentExpression(c);
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
                expMaps[predicate.Parameters[0]] = projection.Projector;
                where = this.Visit(predicate.Body);
            }

            bool isFirst = kind.StartsWith("First");
            bool isLast = kind.StartsWith("Last");
            Expression logicalDeleteExp = (isFirst || isLast) ? Expression.Constant(1) : null;
            if (logicalDeleteExp != null || where != null)
            {
                var alias = this.GetNextAlias();
                ProjectedColumns pc = this.ProjectColumns(projection.Projector, alias, projection.Select.Alias);
                projection = new ProjectionExpression(
                    new SelectExpression(alias, pc.Columns, projection.Select, where, null, null, false, null, logicalDeleteExp, null, null, isLast),
                    pc.Projector, isAsync
                    );
            }
            if (isRoot)
            {
                Type elementType = projection.Projector.Type;
                LambdaExpression gator;
                if (isAsync)
                {
#if NETSTANDARD && !NETSTANDARD2_0
                    var p = Expression.Parameter(typeof(IAsyncEnumerable<>).MakeGenericType(elementType), "p");
#else
                    var p = Expression.Parameter(typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(elementType)), "p");
#endif
                    gator = Expression.Lambda(Expression.Call(typeof(Extensions), nameof(Extensions.FirstOrDefaultCoreAsnyc), new Type[] { elementType }, p), p);
                }
                else
                {
                    var p = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(elementType), "p");
                    gator = Expression.Lambda(Expression.Call(typeof(Enumerable), kind, new Type[] { elementType }, p), p);
                }

                return new ProjectionExpression(projection.Select, projection.Projector, gator, isAsync);
            }
            return projection;
        }

        private Expression BindAnyAll(Expression source, MethodInfo method, LambdaExpression predicate, bool isRoot)
        {
            var isAll = method.Name.StartsWith(nameof(Queryable.All));
            if (source is ConstantExpression constSource && !IsQueryable(constSource))
            {
                System.Diagnostics.Debug.Assert(!isRoot);
                Expression where = null;
                var er = (IEnumerable)constSource.Value;
                foreach (var value in er)
                {
                    Expression expr = Expression.Invoke(predicate, Expression.Constant(value, predicate.Parameters[0].Type));
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
                if (syntax.SupportSubqueryInSelectWithoutFrom)
                {
                    return GetSingletonSequence(result, nameof(Queryable.SingleOrDefault));
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
                        newSelect, exp, QueryUtility.GetAggregator(typeof(bool), typeof(IEnumerable<bool>)), projection.IsAsync
                        );
                }
            }
            return result;
        }

        private Expression GetSingletonSequence(Expression expr, string aggregator)
        {
            LambdaExpression gator = null;
            if (aggregator != null)
            {
#if NETSTANDARD && !NETSTANDARD2_0
                var p = Expression.Parameter(typeof(IAsyncEnumerable<>).MakeGenericType(expr.Type), "p");
                gator = Expression.Lambda(Expression.Call(typeof(Extensions), nameof(Extensions.SingleOrDefaultCoreAsnyc), new[] { expr.Type }, p), p);
#else
                var p = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(expr.Type), "p");
                gator = Expression.Lambda(Expression.Call(typeof(Enumerable), aggregator, new[] { expr.Type }, p), p);
#endif
            }
            var alias = GetNextAlias();
            var select = new SelectExpression(alias, new[] { new ColumnDeclaration("v", expr) }, null, null);
            return new ProjectionExpression(select, new ColumnExpression(expr.Type, alias, "v", null), gator, isQueryAsync);
        }

        private Expression BindContains(Expression source, Expression match, bool isRoot)
        {
            if (source is ConstantExpression constSource && !IsQueryable(constSource))
            {
                var elementType = constSource.Type.GetEnumerableElementType();
                var er = (IEnumerable)constSource.Value;
                if (er.IsNullOrEmpty())
                {
                    return Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(1), Expression.Constant(0));
                }
                if (typeof(IEntity).IsAssignableFrom(elementType))
                {
                    return BindEntityContains(er, match, elementType);
                }

                var values = (from object s in er select Expression.Constant(s.ToType(match.Type), match.Type)).Cast<Expression>().ToList();
                match = Visit(match);
                return new InExpression(match, values);
            }
            else if (isRoot)
            {
                var p = Expression.Parameter(source.Type.GetEnumerableElementType(), "x");
                var predicate = Expression.Lambda(p.Equal(match), p);
                var exp = Expression.Call(typeof(Queryable), nameof(Queryable.Any), new Type[] { p.Type }, source, predicate);
                root = exp;
                return this.Visit(exp);
            }

            if (source is MethodCallExpression mcallExp && mcallExp.Arguments[0].NodeType == ExpressionType.Constant)
            {
                constSource = mcallExp.Arguments[0] as ConstantExpression;
                if (!IsQueryable(constSource))
                {
                }
            }

            //属性是一个集合的情况
            if (source is MemberExpression mbrExp && source.Type.IsArray || 
                (source.Type.IsGenericParameter &&
                    (source.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || source.Type.GetGenericTypeDefinition() == typeof(List<>))))
            {
                return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { source.Type.GetEnumerableElementType() }, Visit(source), Visit(match));
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
            Expression result = new InExpression(match, projection.Select);
            return isRoot ? GetSingletonSequence(result, nameof(Queryable.SingleOrDefault)) : result;
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
            Expression binExp = null;
            //使用所有主键进行匹配
            var properties = PropertyUnity.GetPrimaryProperties(entityType);
            foreach (var property in properties)
            {
                var values = (from object s in enumerable select Expression.Constant(s.As<IEntity>().GetValue(property))).Cast<Expression>().ToList();
                var matchExp = Visit(Expression.Property(parExp, property.Name));
                var inExp = new InExpression(matchExp, values);
                binExp = binExp == null ? (Expression)inExp : Expression.And(binExp, inExp);
            }
            if (binExp == null)
            {
                throw new Exception(SR.GetString(SRKind.NotDefinedPrimaryKey));
            }
            return binExp;
        }

        /// <summary>
        /// 绑定 Delete 命令。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="logicalDeleteExp"></param>
        /// <returns></returns>
        private Expression BindDelete(Expression source, LambdaExpression predicate, Expression logicalDeleteExp, bool isAsync)
        {
            var projection = VisitSequence(source);

            var entityType = predicate.Parameters[0].Type;
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            var logicalDelete = (bool)((ConstantExpression)logicalDeleteExp).Value;
            predicate = (LambdaExpression)Visit(predicate);
            if (metadata.DeleteProperty != null && logicalDelete)
            {
                return Visit(QueryUtility.GetLogicalDeleteExpression(metadata, predicate, isAsync));
            }
            return Visit(QueryUtility.GetDeleteExpression(metadata, predicate, true, isAsync));
        }

        private Expression BindDelete(Expression source, ParameterExpression parExp, Expression logicalDeleteExp, bool isAsync)
        {
            var projection = VisitSequence(source);

            var metadata = EntityMetadataUnity.GetEntityMetadata(parExp.Type);
            var logicalDelete = (bool)((ConstantExpression)logicalDeleteExp).Value;
            var predicate = QueryUtility.GetPrimaryKeyExpression(parExp);
            if (metadata.DeleteProperty != null && logicalDelete)
            {
                return Visit(QueryUtility.GetLogicalDeleteExpression(metadata, predicate, isAsync));
            }
            return Visit(QueryUtility.GetDeleteExpression(metadata, predicate, false, isAsync));
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
            var projection = VisitSequence(source);
            if (instance is ParameterExpression)
            {
                if (predicate == null)
                {
                    predicate = QueryUtility.GetPrimaryKeyExpression((ParameterExpression)instance);
                }
                else
                {
                    predicate = (LambdaExpression)PredicateReplacer.Replace(predicate, source, predicate.Parameters[0]);
                }
            }
            else if (instance is ConstantExpression)
            {
                predicate = BindConcurrencyLockingExpression((ConstantExpression)instance, predicate);
            }

            predicate = (LambdaExpression)Visit(predicate);

            return Visit(QueryUtility.GetUpdateExpression(instance, predicate, isAsync));
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
                var equal = Expression.Equal(Expression.MakeMemberAccess(parExp, property.Info.ReflectionInfo), cvtValue);
                body = Expression.And(body, equal);
            }

            return Expression.Lambda(body, parExp);
        }

        private Expression BindInsert(Expression source, Expression instance, bool isAsync)
        {
            var projection = VisitSequence(source);
            return Visit(QueryUtility.GetInsertExpression(syntax, instance, isAsync));
        }

        private Expression CheckAssociatedProperties(CommandExpression command, Expression instance)
        {
            if (instance is ParameterExpression)
            {
                return command;
            }

            var entity = instance.As<ConstantExpression>().Value as IEntity;
            var blocks = new List<CommandExpression>();

            foreach (var property in PropertyUnity.GetRelatedProperties(entity.EntityType))
            {
                if (entity.IsModified(property.Name))
                {
                    var value = entity.GetValue(property);
                    if (PropertyValue.IsEmpty(value))
                    {
                        continue;
                    }

                    var relationType = (property as RelationProperty).RelationalType;
                    var source = Expression.Constant(TranslateScope.Current.ContextService.GetDbSet(relationType));

                    if (property is EntitySetProperty)
                    {
                        var es = value.GetValue() as IEntitySet;
                        foreach (IEntity p1 in es)
                        {
                            var e1 = GetEntityOper(source, p1);
                            if (e1 != null)
                            {
                                blocks.Add(e1);
                            }
                        }
                    }
                    else if (property is EntityProperty)
                    {
                        var re = value.GetValue() as IEntity;
                        var e1 = GetEntityOper(source, re);
                        if (e1 != null)
                        {
                            blocks.Add(e1);
                        }
                    }
                }
            }

            if (blocks.Count > 0)
            {
                command.AssociatedCommands.AddRange(blocks);
            }

            return command;
        }

        private CommandExpression GetEntityOper(Expression source, IEntity entity)
        {
            switch (entity.EntityState)
            {
                case EntityState.Attached:
                    return (CommandExpression)BindInsert(source, Expression.Constant(entity), false);
                case EntityState.Modified:
                    return (CommandExpression)BindUpdate(source, Expression.Constant(entity), null, false);
                case EntityState.Detached:
                default:
                    return null;
            }
        }

        private Expression BindBatch(Expression source, Expression instances, LambdaExpression operation, bool isAsync)
        {
            var projection = VisitSequence(source);

            var arguments = QueryUtility.AttachModifiedProperties(instances);

            var op = (LambdaExpression)this.Visit(operation);

            QueryUtility.ReleaseModifiedProperties();

            var items = this.Visit(instances);
            return new BatchCommandExpression(items, op, isAsync, arguments);
        }

        public Expression BindExtend(Expression source, LambdaExpression selector)
        {
            List<Expression> arguments = new List<Expression>();
            List<MemberInfo> members = new List<MemberInfo>();

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
                        var value = property.GetValue(obj, null);
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
                var columnExp = new ColumnExpression(property.Type, __alias, property.Name, property.Info);
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
            var memberInits = new List<MemberAssignment>();

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
                        memberInits.Add(m as MemberAssignment);
                    }
                }
                else if (expression.NodeType == ExpressionType.Constant)
                {
                    var obj = (expression as ConstantExpression).Value;
                    foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var value = property.GetValue(obj, null);
                        if (value != null)
                        {
                            memberInits.Add(Expression.Bind(property, Expression.Constant(value, property.PropertyType)));
                        }
                    }
                }
            }

            foreach (var property in PropertyUnity.GetPersistentProperties(returnType))
            {
                var prop = returnType.GetProperty(property.Name);
                if (prop == null || !prop.CanWrite)
                {
                    continue;
                }

                var columnExp = new ColumnExpression(property.Type, __alias, property.Name, property.Info);
                memberInits.Add(Expression.Bind(prop, columnExp));
            }

            return Expression.MemberInit(Expression.New(returnType), memberInits.Cast<MemberBinding>());
        }

        public Expression BindTo(Type returnType, Expression source)
        {
            var memberInits = new List<MemberAssignment>();
            foreach (var property in PropertyUnity.GetPersistentProperties(returnType))
            {
                var prop = returnType.GetProperty(property.Name);
                if (prop == null || !prop.CanWrite)
                {
                    continue;
                }

                var columnExp = new ColumnExpression(property.Type, __alias, property.Name, property.Info);
                memberInits.Add(Expression.Bind(prop, columnExp));
            }

            return Expression.MemberInit(Expression.New(returnType), memberInits.Cast<MemberBinding>());
        }

        /// <summary>
        /// 判断是否为 IQueryable
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool IsQueryable(Expression expression)
        {
            var elementType = expression.Type.GetEnumerableElementType();
            return elementType != null &&
                typeof(IQueryable<>).MakeGenericType(elementType).IsAssignableFrom(expression.Type);
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
                    var mex = result as MemberExpression;
                    if (mex != null &&
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
                        var assign = min.Bindings[i] as MemberAssignment;
                        if (assign != null &&
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
                    return new ProjectionExpression(proj.Select, newProjector, QueryUtility.GetAggregator(mt, typeof(IEnumerable<>).MakeGenericType(mt)), proj.IsAsync);

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

                var attr = a.GetCustomAttributes<InterfaceMemberMappingAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    return attr.Type == b.DeclaringType && attr.Name == a.Name;
                }
            }
            return false;
        }

        private class PredicateReplacer : Common.Linq.Expressions.ExpressionVisitor
        {
            private Expression source;
            private ParameterExpression parExp;
            private TableExpression table;

            protected PredicateReplacer(Expression source, ParameterExpression parExp)
            {
                this.source = source;
                this.parExp = parExp;

                var metadata = EntityMetadataUnity.GetEntityMetadata(parExp.Type);
                table = new TableExpression(new TableAlias(), metadata.TableName, parExp.Type);
            }

            public static Expression Replace(Expression expression, Expression source, ParameterExpression parExp)
            {
                return new PredicateReplacer(source, parExp).Visit(expression);
            }

            protected override Expression VisitMember(MemberExpression memberExp)
            {
                if (memberExp.Expression == parExp)
                {
                    var property = PropertyUnity.GetProperty(memberExp.Member.DeclaringType, memberExp.Member.Name);
                    if (property == null)
                    {
                        return memberExp;
                    }

                    return QueryUtility.GetMemberExpression(table, property);
                }

                return memberExp;
            }
        }

        private class ParameterTypeFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private Type parameterType;

            public static Type Find(Expression expression)
            {
                var finder = new ParameterTypeFinder();
                finder.Visit(expression);
                return finder.parameterType;
            }

            protected override Expression VisitParameter(ParameterExpression parExp)
            {
                parameterType = parExp.Type;
                return parExp;
            }
        }
    }
}