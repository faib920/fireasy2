// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 一个抽象类，用于将 ELinq 表达式翻译为 SQL 语句。
    /// </summary>
    public abstract class TranslatorBase : DbExpressionVisitor
    {
        private Stack<string> stack = new Stack<string>();
        private StringBuilder builder;
        private const int Indent = 2;
        private int dept;
        private Dictionary<TableAlias, string> aliases;
        private IDataSegment dataSegment;

        /// <summary>
        /// 获取或设置是否嵌套查询。
        /// </summary>
        protected bool IsNested;

        /// <summary>
        /// 初始化 <see cref="TranslatorBase"/> 类的新实例。
        /// </summary>
        protected TranslatorBase()
        {
            builder = new StringBuilder();
            aliases = new Dictionary<TableAlias, string>();
            Parameters = new ParameterCollection();

            if (TranslateScope.Current != null)
            {
                Options = TranslateScope.Current.Options;
                Syntax = TranslateScope.Current.ContextService.Provider.GetService<ISyntaxProvider>();
                Environment = (TranslateScope.Current.ContextService as IEntityPersistentEnvironment)?.Environment;
            }
        }

        /// <summary>
        /// 获取或设置语法插件服务。
        /// </summary>
        public ISyntaxProvider Syntax { get; set; }

        /// <summary>
        /// 获取或设置翻译器的选项。
        /// </summary>
        public TranslateOptions Options { get; set; }

        /// <summary>
        /// 获取参数集合。
        /// </summary>
        protected ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// 获取或设置持久化环境。
        /// </summary>
        public EntityPersistentEnvironment Environment { get; set; }

        /// <summary>
        /// 执行表达式的翻译。
        /// </summary>
        /// <param name="expression">查询表达式。</param>
        /// <returns>翻译结果对象。</returns>
        public TranslateResult Translate(Expression expression)
        {
            builder = new StringBuilder();
            Parameters.Clear();

            Visit(expression);

            return new TranslateResult
            {
                QueryText = ToString(),
                Syntax = Syntax,
                DataSegment = dataSegment,
                Parameters = Parameters
            };
        }

        /// <summary>
        /// 将表达式翻译成字符串。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string TranslateString(Expression expression)
        {
            stack.Push(builder.ToString());
            builder.Length = 0;
            Visit(expression);
            var str = builder.ToString();
            builder = new StringBuilder(stack.Pop());
            return str;
        }

        /// <summary>
        /// 输出翻译的查询SQL。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return builder.ToString();
        }

        /// <summary>
        /// 向构造器中添加新行。
        /// </summary>
        /// <param name="style">缩进样式。</param>
        protected void WriteLine(Indentation style)
        {
            builder.AppendLine();
            SetIndentation(style);
            builder.Append(new string(' ', dept * Indent));
        }

        /// <summary>
        /// 向构造器中添加值。
        /// </summary>
        /// <param name="value"></param>
        protected void Write(object value)
        {
            builder.Append(value);
        }

        /// <summary>
        /// 控制构造器的缩进
        /// </summary>
        /// <param name="style"></param>
        protected void SetIndentation(Indentation style)
        {
            switch (style)
            {
                case Indentation.Inner:
                    dept++;
                    break;
                case Indentation.Outer:
                    dept--;
                    break;
            }
        }

        #region 表达式的访问
        public override Expression Visit(Expression exp)
        {
            if (exp == null) return null;

            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.MemberAccess:
                case ExpressionType.Call:
                case ExpressionType.New:
                case (ExpressionType)DbExpressionType.Table:
                case (ExpressionType)DbExpressionType.ClientJoin:
                case (ExpressionType)DbExpressionType.Column:
                case (ExpressionType)DbExpressionType.Select:
                case (ExpressionType)DbExpressionType.Join:
                case (ExpressionType)DbExpressionType.Aggregate:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.In:
                case (ExpressionType)DbExpressionType.AggregateSubquery:
                case (ExpressionType)DbExpressionType.IsNull:
                case (ExpressionType)DbExpressionType.Between:
                case (ExpressionType)DbExpressionType.RowCount:
                case (ExpressionType)DbExpressionType.NamedValue:
                case (ExpressionType)DbExpressionType.Projection:
                case (ExpressionType)DbExpressionType.Function:
                case (ExpressionType)DbExpressionType.Segment:
                case (ExpressionType)DbExpressionType.Block:
                case (ExpressionType)DbExpressionType.Generator:
                case (ExpressionType)DbExpressionType.SqlText:
                    return base.Visit(exp);
                case (ExpressionType)DbExpressionType.Delete:
                case (ExpressionType)DbExpressionType.Update:
                case (ExpressionType)DbExpressionType.Insert:
                    return HideAliases(() => base.Visit(exp));
                default:
                    throw new TranslateException(exp, new Exception(SR.GetString(SRKind.NodeTranslateNotSupported, (DbExpressionType)exp.NodeType)));
            }
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression is MemberExpression parMember &&
                parMember.Member.DeclaringType.IsNullableType())
            {
                var value = Expression.MakeMemberAccess(Expression.Convert(parMember.Expression, m.Member.DeclaringType), m.Member);
                return Visit(value);
            }

            if (m.Expression is ColumnExpression columnExp &&
                columnExp.Type.IsNullableType())
            {
                var value = Expression.Convert(columnExp, columnExp.Type.GetNullableType());
                return Visit(value);
            }

            if (m.Member.DeclaringType == typeof(string))
            {
                return VisitStringMember(m);
            }
            else if (m.Member.DeclaringType == typeof(DateTime) ||
                     m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                return VisitDateTimeMember(m);
            }
            else if (m.Member.DeclaringType == typeof(TimeSpan))
            {
                return VisitTimeSpanMember(m);
            }

            throw new TranslateException(m, new Exception(SR.GetString(SRKind.MemberTranslateNotSupported, m.Member.Name)));
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if ((m.Method.Name == nameof(int.CompareTo) || m.Method.Name == "Compare") &&
                m.Method.ReturnType == typeof(int))
            {
                return VisitCompareMethod(m);
            }

            if (m.Method.DeclaringType == typeof(string) || m.Method.DeclaringType == typeof(StringExtension))
            {
                return VisitStringMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                return VisitDateTimeMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Decimal))
            {
                return VisitDecimalMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Math))
            {
                return VisitMathMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Regex))
            {
                return VisitRegexMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Convert))
            {
                return VisitConvertMethod(m);
            }
            else
            {
                return VisitOtherMethod(m);
            }
        }

        protected override Expression VisitNew(NewExpression nex)
        {
            if (nex.Constructor.DeclaringType == typeof(DateTime))
            {
                if (nex.Arguments.Count == 3)
                {
                    Write(Syntax.DateTime.New(
                        TranslateString(nex.Arguments[0]),
                        TranslateString(nex.Arguments[1]),
                        TranslateString(nex.Arguments[2])));

                    return nex;
                }
                else if (nex.Arguments.Count == 6)
                {
                    Write(Syntax.DateTime.New(
                        TranslateString(nex.Arguments[0]),
                        TranslateString(nex.Arguments[1]),
                        TranslateString(nex.Arguments[2]),
                        TranslateString(nex.Arguments[3]),
                        TranslateString(nex.Arguments[4]),
                        TranslateString(nex.Arguments[5])));

                    return nex;
                }
            }

            throw new TranslateException(nex, new NotSupportedException(SR.GetString(SRKind.NewTranslateNotSupported, nex.Constructor)));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            var op = GetOperator(u);

            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    if (IsBoolean(u.Operand.Type))
                    {
                        Write(op);
                        Write(" ");
                        VisitPredicate(u.Operand);
                    }
                    else
                    {
                        Write(Syntax.Math.Not(TranslateString(u.Operand)));
                    }
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    Write(Syntax.Math.Negate(TranslateString(u.Operand)));
                    break;
                case ExpressionType.UnaryPlus:
                    VisitValue(u.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new TranslateException(u, new NotSupportedException(SR.GetString(SRKind.UnaryTranslateNotSupported, u.NodeType)));
            }

            return u;
        }
        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            // convert client join into a up-front lookup table builder & replace client-join in tree with lookup accessor

            // 1) lookup = query.Select(e => new KVP(key: inner, value: e)).ToLookup(kvp => kvp.Key, kvp => kvp.Value)
            var innerKey = MakeJoinKey(join.InnerKey);
            var outerKey = MakeJoinKey(join.OuterKey);

            var kvpConstructor = typeof(KeyValuePair<,>).MakeGenericType(innerKey.Type, join.Projection.Projector.Type).GetConstructor(new Type[] { innerKey.Type, join.Projection.Projector.Type });
            var constructKVPair = Expression.New(kvpConstructor, innerKey, join.Projection.Projector);
            Expression newProjection = new ProjectionExpression(join.Projection.Select, constructKVPair, false);

            var kvp = Expression.Parameter(constructKVPair.Type, "kvp");

            // filter out nulls
            if (join.Projection.Projector.NodeType == (ExpressionType)DbExpressionType.OuterJoined)
            {
                var pred = Expression.Lambda(
                    Expression.NotEqual(Expression.PropertyOrField(kvp, "Value"), Expression.Constant(null, join.Projection.Projector.Type.GetNullableType())),
                    kvp
                    );

                newProjection = Expression.Call(typeof(Enumerable), nameof(Enumerable.Where), new Type[] { kvp.Type }, newProjection, pred);
            }

            // make lookup
            var keySelector = Expression.Lambda(Expression.PropertyOrField(kvp, "Key"), kvp);
            var elementSelector = Expression.Lambda(Expression.PropertyOrField(kvp, "Value"), kvp);
            var toLookup = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToLookup), new Type[] { kvp.Type, outerKey.Type, join.Projection.Projector.Type }, newProjection, keySelector, elementSelector);

            // 2) agg(lookup[outer])
            ParameterExpression lookup = Expression.Parameter(toLookup.Type, "lookup");
            var property = lookup.Type.GetProperty("Item");
            Expression access = Expression.Call(lookup, property.GetGetMethod(), this.Visit(outerKey));
            if (join.Projection.Aggregator != null)
            {
                // apply aggregator
                access = DbExpressionReplacer.Replace(join.Projection.Aggregator.Body, join.Projection.Aggregator.Parameters[0], access);
            }

            return access;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            var left = b.Left;
            var right = b.Right;

            if (b.NodeType == ExpressionType.Power)
            {
                return VisitBinaryPower(b);
            }

            if (b.NodeType == ExpressionType.Coalesce)
            {
                return VisitBinaryCoalesce(b);
            }

            Write("(");

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    VisitBinaryLogic(b);
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (VisitBinaryEqual(b) == null)
                    {
                        break;
                    }
                    goto case ExpressionType.LessThan;
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    VisitBinaryCompare(b, ref left, ref right);
                    goto case ExpressionType.Subtract;
                case ExpressionType.Modulo:
                    Write(Syntax.Math.Modulo(TranslateString(left),
                                                TranslateString(right)));
                    break;
                case ExpressionType.ExclusiveOr:
                    Write(Syntax.Math.ExclusiveOr(TranslateString(left),
                                                     TranslateString(right)));
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    if (IsString(left) || IsString(right))
                    {
                        Write(Syntax.String.Concat(TranslateString(left),
                                                      TranslateString(right)));
                        break;
                    }
                    goto case ExpressionType.Subtract;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                    VisitValue(left);
                    Write(" ");
                    Write(GetOperator(b));
                    Write(" ");
                    VisitValue(right);
                    break;
                case ExpressionType.LeftShift:
                    Write(Syntax.Math.LeftShift(TranslateString(left),
                                                   TranslateString(right)));
                    break;
                case ExpressionType.RightShift:
                    Write(Syntax.Math.RightShift(TranslateString(left),
                                                    TranslateString(right)));
                    break;
                default:
                    throw new TranslateException(b, new NotSupportedException(SR.GetString(SRKind.UnaryTranslateNotSupported, b.NodeType)));
            }

            Write(")");

            return b;
        }

        protected virtual Expression VisitPredicate(Expression expr)
        {
            Visit(expr);

            if (!IsPredicate(expr))
            {
                Write(" <> 0");
            }

            return expr;
        }

        protected virtual Expression VisitValue(Expression expr)
        {
            if (IsPredicate(expr))
            {
                Write("CASE WHEN (");
                Visit(expr);
                Write(") THEN 1 ELSE 0 END");
            }
            else
            {
                Visit(expr);
            }

            return expr;
        }

        /// <summary>
        /// 对条件的翻译。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected override Expression VisitConditional(ConditionalExpression c)
        {
            if (IsPredicate(c.Test))
            {
                Write("(CASE WHEN ");
                VisitPredicate(c.Test);
                Write(" THEN ");
                VisitValue(c.IfTrue);
                var ifFalse = c.IfFalse;

                while (ifFalse != null && ifFalse.NodeType == ExpressionType.Conditional)
                {
                    var fc = (ConditionalExpression)ifFalse;
                    Write(" WHEN ");
                    VisitPredicate(fc.Test);
                    Write(" THEN ");
                    VisitValue(fc.IfTrue);
                    ifFalse = fc.IfFalse;
                }

                if (ifFalse != null)
                {
                    Write(" ELSE ");
                    VisitValue(ifFalse);
                }

                Write(" END)");
            }
            else
            {
                Write("(CASE ");
                VisitValue(c.Test);
                Write(" WHEN 0 THEN ");
                VisitValue(c.IfFalse);
                Write(" ELSE ");
                VisitValue(c.IfTrue);
                Write(" END)");
            }
            return c;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (Options.AttachParameter &&
                Parameters != null)
            {
                if (c.Value == null)
                {
                    Write("NULL");
                    return c;
                }

                if (IsStringOrDate(c) || c.Value is PropertyValue)
                {
                    var parameter = new Parameter("p" + Parameters.Count);

                    if (c.Value is PropertyValue pv)
                    {
                        PropertyValue.Parametrization(pv, parameter);
                    }
                    else
                    {
                        var converter = ConvertManager.GetConverter(c.Type);
                        var value = converter != null ? converter.ConvertTo(c.Value) : c.Value;
                        parameter.Value = value;
                    }

                    Parameters.Add(parameter);

                    Write(Syntax.FormatParameter(parameter.ParameterName));

                    return c;
                }
            }

            WriteValue(c.Value);
            return c;
        }

        protected virtual void WriteValue(object value)
        {
            if (value == null)
            {
                Write("NULL");
            }
            else
            {
                if (value.GetType().IsEnum)
                {
                    Write(value.To<int>());
                    return;
                }
                else if (value is PropertyValue)
                {
                    WriteValue(((PropertyValue)value).GetValue());
                }

                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        Write(((bool)value) ? 1 : 0);
                        break;
                    case TypeCode.Char:
                    case TypeCode.String:
                        if (Options.AttachParameter)
                        {
                            Write(value);
                        }
                        else
                        {
                            Write("'" + value + "'");
                        }
                        break;
                    case TypeCode.DateTime:
                        Write(Options.AttachParameter ? value : Syntax.Convert("'" + value + "'", DbType.DateTime));
                        break;
                    case TypeCode.Object:
                        throw new TranslateException(null, new NotSupportedException(SR.GetString(SRKind.ValueTranslateNotSupported, value)));
                    default:
                        Write(value);
                        break;
                }
            }
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            var sqc = column as SubqueryColumnExpression;
            if (sqc == null)
            {
                if (column.Alias != null && !Options.HideColumnAliases)
                {
                    Write(GetAliasName(column.Alias));
                    Write(".");
                }

                Write(GetColumnName(column.MapInfo == null ? column.Name : column.MapInfo.FieldName));
            }
            else
            {
                var alias = string.Empty;
                if (column.Alias != null && !Options.HideColumnAliases)
                {
                    alias = GetAliasName(column.Alias) + ".";
                }

                Write("(" + sqc.Subquery.Replace("$", alias) + ")");
            }
            return column;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            if (proj.Projector is ColumnExpression)
            {
                Write("(");
                WriteLine(Indentation.Inner);
                Visit(proj.Select);
                Write(")");
                SetIndentation(Indentation.Outer);
            }
            else
            {
                throw new TranslateException(proj, new NotSupportedException(SR.GetString(SRKind.ScalarTranslateNotSupported)));
            }
            return proj;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            Write("SELECT ");
            if (select.IsDistinct)
            {
                Write("DISTINCT ");
            }

            if (select.Take != null)
            {
                Write("TOP ");
                Visit(select.Take);
                Write(" ");
            }

            WriteColumns(select.Columns);

            if (select.From != null)
            {
                WriteLine(Indentation.Same);
                Write("FROM ");
                VisitSource(select.From);
            }
            else
            {
                WriteLine(Indentation.Same);
                Write(Syntax.FakeSelect);
            }

            if (select.Where != null)
            {
                WriteLine(Indentation.Same);
                Write("WHERE ");
                VisitPredicate(select.Where);
            }

            if (select.GroupBy != null)
            {
                WriteGroups(select);
            }

            if (select.Having != null)
            {
                WriteLine(Indentation.Same);
                Write("HAVING ");
                VisitPredicate(select.Where);
            }

            if (select.OrderBy != null)
            {
                WriteOrders(select);
            }

            return select;
        }

        protected override Expression VisitSource(Expression source)
        {
            bool saveIsNested = IsNested;
            IsNested = true;

            switch ((DbExpressionType)source.NodeType)
            {
                case DbExpressionType.Table:
                    var table = (TableExpression)source;
                    var tableName = GetTableName(table);
                    Write(DbUtility.FormatByQuote(Syntax, tableName));

                    if (!Options.HideTableAliases)
                    {
                        WriteAs();
                        Write(GetAliasName(table.Alias));
                    }

                    break;
                case DbExpressionType.Select:
                    var select = (SelectExpression)source;
                    Write("(");
                    WriteLine(Indentation.Inner);
                    Visit(select);
                    WriteLine(Indentation.Same);
                    Write(")");
                    WriteAs();
                    Write(GetAliasName(select.Alias));
                    SetIndentation(Indentation.Outer);
                    break;
                case DbExpressionType.Join:
                    VisitJoin((JoinExpression)source);
                    break;
                default:
                    throw new TranslateException(source, new InvalidOperationException(SR.GetString(SRKind.EntityQueryInvalid)));
            }

            IsNested = saveIsNested;
            return source;
        }

        /// <summary>
        /// 对JOIN子句的翻译。
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        protected override Expression VisitJoin(JoinExpression join)
        {
            VisitSource(join.Left);
            WriteLine(Indentation.Same);

            switch (join.JoinType)
            {
                case JoinType.CrossJoin:
                    Write("CROSS JOIN ");
                    break;
                case JoinType.InnerJoin:
                    Write("INNER JOIN ");
                    break;
                case JoinType.CrossApply:
                    Write("CROSS APPLY ");
                    break;
                case JoinType.OuterApply:
                    Write("OUTER APPLY ");
                    break;
                case JoinType.LeftOuter:
                    Write("LEFT OUTER JOIN ");
                    break;
                case JoinType.RightOuter:
                    Write("RIGHT OUTER JOIN ");
                    break;
            }

            VisitSource(join.Right);

            if (join.Condition != null)
            {
                WriteLine(Indentation.Inner);
                Write("ON ");
                VisitPredicate(join.Condition);
                SetIndentation(Indentation.Outer);
            }

            return join;
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            Write(GetAggregateName(aggregate.AggregateType));
            Write("(");

            if (aggregate.IsDistinct)
            {
                Write("DISTINCT ");
            }

            if (aggregate.Argument != null)
            {
                VisitValue(aggregate.Argument);
            }
            else if (RequiresAsteriskWhenNoArgument(aggregate.AggregateType))
            {
                Write("*");
            }

            Write(")");

            return aggregate;
        }

        protected override Expression VisitIsNull(IsNullExpression isnull)
        {
            VisitValue(isnull.Expression);
            Write(" IS NULL");

            return isnull;
        }

        protected override Expression VisitBetween(BetweenExpression between)
        {
            VisitValue(between.Argument);
            Write(" BETWEEN ");
            VisitValue(between.Lower);
            Write(" AND ");
            VisitValue(between.Upper);

            return between;
        }

        protected override Expression VisitScalar(ScalarExpression subquery)
        {
            Write("(");
            WriteLine(Indentation.Inner);
            Visit(subquery.Select);
            WriteLine(Indentation.Same);
            Write(")");
            SetIndentation(Indentation.Outer);

            return subquery;
        }

        protected override Expression VisitSegment(SegmentExpression segment)
        {
            dataSegment = segment.Segment;
            return segment;
        }

        /// <summary>
        /// 对EXISTS子查询的翻译。
        /// </summary>
        /// <param name="exists"></param>
        /// <returns></returns>
        protected override Expression VisitExists(ExistsExpression exists)
        {
            Write("EXISTS(");
            WriteLine(Indentation.Inner);
            Visit(exists.Select);
            WriteLine(Indentation.Same);
            Write(")");
            SetIndentation(Indentation.Outer);

            return exists;
        }

        protected override Expression VisitIn(InExpression @in)
        {
            if (@in.Select != null)
            {
                VisitValue(@in.Expression);
                Write(" IN (");
                WriteLine(Indentation.Inner);
                Visit(@in.Select);
                WriteLine(Indentation.Same);
                Write(")");
                SetIndentation(Indentation.Outer);
            }
            else if (@in.Values != null)
            {
                if (@in.Values.Count == 0)
                {
                    Write("0 <> 0");
                }
                else
                {
                    VisitValue(@in.Expression);
                    Write(" IN (");
                    for (int i = 0, n = @in.Values.Count; i < n; i++)
                    {
                        if (i > 0) Write(", ");
                        VisitValue(@in.Values[i]);
                    }
                    Write(")");
                }
            }

            return @in;
        }

        protected override Expression VisitFunction(FunctionExpression func)
        {
            Write(func.Name);

            if (func.Arguments.Count > 0)
            {
                Write("(");

                for (int i = 0, n = func.Arguments.Count; i < n; i++)
                {
                    if (i > 0)
                    {
                        Write(", ");
                    }
                    Visit(func.Arguments[i]);
                }

                Write(")");
            }

            return func;
        }

        protected override Expression VisitNamedValue(NamedValueExpression value)
        {
            Write(Syntax.FormatParameter(value.Name));
            return value;
        }

        protected override Expression VisitDelete(DeleteCommandExpression delete)
        {
            if (delete.Table != null)
            {
                Write("DELETE FROM ");
                VisitSource(delete.Table);
            }

            if (delete.Where != null)
            {
                WriteLine(Indentation.Same);
                Write("WHERE ");
                VisitPredicate(delete.Where);
            }

            return delete;
        }

        protected override Expression VisitUpdate(UpdateCommandExpression update)
        {
            if (update.Table != null)
            {
                Write("UPDATE ");
                VisitSource(update.Table);
            }

            WriteLine(Indentation.Same);
            Write("SET");
            WriteLine(Indentation.Inner);

            var flag = new AssertFlag();
            foreach (var assignment in update.Assignments)
            {
                if (!flag.AssertTrue())
                {
                    Write(", ");
                    WriteLine(Indentation.Same);
                }

                Visit(assignment.Column);
                Write(" = ");
                Visit(assignment.Expression);
            }

            WriteLine(Indentation.Outer);

            if (update.Where != null)
            {
                Write("WHERE ");
                VisitPredicate(update.Where);
            }

            return update;
        }

        protected override Expression VisitInsert(InsertCommandExpression insert)
        {
            if (string.IsNullOrEmpty(Syntax.IdentitySelect))
            {
                insert.WithAutoIncrement = false;
            }

            if (insert.Table != null)
            {
                Write("INSERT INTO ");
                VisitSource(insert.Table);
            }

            WriteLine(Indentation.Same);
            Write("(");
            WriteLine(Indentation.Inner);

            var flag = new AssertFlag();
            foreach (var assignment in insert.Assignments)
            {
                if (!flag.AssertTrue())
                {
                    Write(", ");
                    WriteLine(Indentation.Same);
                }

                Visit(assignment.Column);
            }

            WriteLine(Indentation.Outer);
            Write(") VALUES (");
            WriteLine(Indentation.Inner);

            flag.Reset();
            foreach (var assignment in insert.Assignments)
            {
                if (!flag.AssertTrue())
                {
                    Write(", ");
                    WriteLine(Indentation.Same);
                }

                Visit(assignment.Expression);
            }

            WriteLine(Indentation.Outer);
            Write(")");

            if (insert.WithAutoIncrement)
            {
                Write(";\n");
                Write(Syntax.IdentitySelect);
            }

            return insert;
        }

        protected override Expression VisitBlock(BlockCommandExpression block)
        {
            var assert = new AssertFlag();
            foreach (var cmd in block.Commands)
            {
                if (!assert.AssertTrue())
                {
                    Write(";");
                }

                Visit(cmd);
                WriteLine(Indentation.Same);
            }

            return block;
        }

        #endregion

        #region 私有方法
        protected virtual string GetOperator(string methodName)
        {
            switch (methodName)
            {
                case nameof(decimal.Add):
                    return " +";
                case nameof(decimal.Subtract):
                    return "-";
                case nameof(decimal.Multiply):
                    return "*";
                case nameof(decimal.Divide):
                    return "/";
                case nameof(decimal.Negate):
                    return "-";
                case nameof(decimal.Remainder):
                    return "%";
                default:
                    return null;
            }
        }

        protected virtual string GetOperator(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return "-";
                case ExpressionType.UnaryPlus:
                    return "+";
                case ExpressionType.Not:
                    return IsBoolean(u.Operand.Type) ? "NOT" : Syntax.Math.Negate("");
                default:
                    return "";
            }
        }

        protected virtual string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return (IsBoolean(b.Left.Type)) ? "AND" : "&";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return (IsBoolean(b.Left.Type) ? "OR" : "|");
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                default:
                    return "";
            }
        }

        protected bool IsBoolean(Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        protected bool IsString(Expression exp)
        {
            return exp.Type == typeof(string) ||
                exp.Type == typeof(char) || exp.Type == typeof(char?) ||
                exp.Type == typeof(Guid);
        }

        protected bool IsStringOrDate(Expression exp)
        {
            return (IsString(exp) ||
                exp.Type == typeof(DateTime) ||
                exp.Type == typeof(DateTime?));
        }

        protected bool IsPredicate(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(expr.Type);
                case ExpressionType.Not:
                    return IsBoolean(expr.Type);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case (ExpressionType)DbExpressionType.IsNull:
                case (ExpressionType)DbExpressionType.Between:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.In:
                    return true;
                case ExpressionType.Call:
                    return IsBoolean(expr.Type);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取表的别名。
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        protected string GetAliasName(TableAlias alias)
        {
            if (!aliases.TryGetValue(alias, out string name))
            {
                name = $"t{aliases.Count}";
                aliases.Add(alias, name);
            }

            return name;
        }

        /// <summary>
        /// 获取聚合函数的名称。
        /// </summary>
        /// <param name="aggregateType"></param>
        /// <returns></returns>
        protected string GetAggregateName(AggregateType aggregateType)
        {
            switch (aggregateType)
            {
                case AggregateType.Count:
                    return "COUNT";
                case AggregateType.Min:
                    return "MIN";
                case AggregateType.Max:
                    return "MAX";
                case AggregateType.Sum:
                    return "SUM";
                case AggregateType.Average:
                    return "AVG";
                default:
                    throw new TranslateException(null, new Exception(SR.GetString(SRKind.UnknowAggregateType, aggregateType)));
            }
        }

        protected virtual Expression VisitCompareMethod(MethodCallExpression m)
        {
            if (!m.Method.IsStatic && m.Arguments.Count == 1)
            {
                Write("(CASE WHEN ");
                Visit(m.Object);
                Write(" = ");
                Visit(m.Arguments[0]);
                Write(" THEN 0 WHEN ");
                Visit(m.Object);
                Write(" < ");
                Visit(m.Arguments[0]);
                Write(" THEN -1 ELSE 1 END)");
            }
            else if (m.Method.IsStatic && m.Arguments.Count == 2)
            {
                Write("(CASE WHEN ");
                Visit(m.Arguments[0]);
                Write(" = ");
                Visit(m.Arguments[1]);
                Write(" THEN 0 WHEN ");
                Visit(m.Arguments[0]);
                Write(" < ");
                Visit(m.Arguments[1]);
                Write(" THEN -1 ELSE 1 END)");
            }

            return m;
        }

        protected virtual Expression VisitStringMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case nameof(string.StartsWith):
                    Write(string.Format("({0} LIKE {1})",
                            TranslateString(m.Object),
                            Syntax.String.Concat(TranslateString(m.Arguments[0]), "'%'"))
                        );
                    break;
                case nameof(string.EndsWith):
                    Write(string.Format("({0} LIKE {1})",
                            TranslateString(m.Object),
                            Syntax.String.Concat("'%'", TranslateString(m.Arguments[0])))
                        );
                    break;
                case nameof(string.Contains):
                    Write(string.Format("({0} LIKE {1})",
                            TranslateString(m.Object),
                            Syntax.String.Concat("'%'", TranslateString(m.Arguments[0]), "'%'"))
                        );
                    break;
                case nameof(string.Concat):
                    IList<Expression> args = m.Arguments;
                    if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                    {
                        args = ((NewArrayExpression)args[0]).Expressions;
                    }

                    var sps = new string[args.Count];
                    for (int i = 0, n = args.Count; i < n; i++)
                    {
                        sps[i] = TranslateString(args[i]);
                    }

                    Write(Syntax.String.Concat(sps));
                    break;
                case nameof(string.IsNullOrEmpty):
                    Write("(");
                    Visit(m.Arguments[0]);
                    Write(" IS NULL OR ");
                    Visit(m.Arguments[0]);
                    Write(" = '')");
                    break;
                case nameof(string.ToUpper):
                    Write(Syntax.String.ToUpper(TranslateString(m.Object)));
                    break;
                case nameof(string.ToLower):
                    Write(Syntax.String.ToLower(TranslateString(m.Object)));
                    break;
                case nameof(string.Replace):
                    Write(Syntax.String.Replace(TranslateString(m.Object),
                        TranslateString(m.Arguments[0]),
                        TranslateString(m.Arguments[1])));
                    break;
                case nameof(string.Substring):
                    Write(Syntax.String.Substring(TranslateString(m.Object),
                        TranslateString(m.Arguments[0]) + " + 1",
                        m.Arguments.Count == 2 ? TranslateString(m.Arguments[1]) : "8000"));
                    break;
                case nameof(string.IndexOf):
                    var startIndex = m.Arguments.Count > 1 ? TranslateString(m.Arguments[1]) : null;
                    var count = m.Arguments.Count > 2 ? TranslateString(m.Arguments[2]) : null;
                    Write(Syntax.String.IndexOf(TranslateString(m.Object),
                        TranslateString(m.Arguments[0]),
                        startIndex, count) + " - 1");
                    break;
                case nameof(string.Trim):
                    Write(Syntax.String.Trim(TranslateString(m.Object)));
                    break;
                case nameof(string.TrimStart):
                    Write(Syntax.String.TrimStart(TranslateString(m.Object)));
                    break;
                case nameof(string.TrimEnd):
                    Write(Syntax.String.TrimEnd(TranslateString(m.Object)));
                    break;
                case nameof(string.Equals):
                    if (m.Method.IsStatic && m.Method.DeclaringType == typeof(object))
                    {
                        Write("(");
                        Visit(m.Arguments[0]);
                        Write(" = ");
                        Visit(m.Arguments[1]);
                        Write(")");
                    }
                    else if (!m.Method.IsStatic && m.Arguments.Count == 1 && m.Arguments[0].Type == m.Object.Type)
                    {
                        Write("(");
                        Visit(m.Object);
                        Write(" = ");
                        Visit(m.Arguments[0]);
                        Write(")");
                    }

                    break;
                case nameof(string.ToString):
                    if (m.Object.Type == typeof(string))
                    {
                        Visit(m.Object);
                    }

                    break;
                case "Like":
                    Visit(m.Arguments[0]);
                    Write(" LIKE ");
                    Visit(m.Arguments[1]);
                    break;
            }

            return m;
        }

        protected virtual Expression VisitDateTimeMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case nameof(DateTime.AddSeconds):
                    Write(Syntax.DateTime.AddSeconds(TranslateString(m.Object), TranslateString(m.Arguments[0])));
                    break;
                case nameof(DateTime.AddMinutes):
                    Write(Syntax.DateTime.AddMinutes(TranslateString(m.Object), TranslateString(m.Arguments[0])));
                    break;
                case nameof(DateTime.AddHours):
                    Write(Syntax.DateTime.AddHours(TranslateString(m.Object), TranslateString(m.Arguments[0])));
                    break;
                case nameof(DateTime.AddDays):
                    Write(Syntax.DateTime.AddDays(TranslateString(m.Object), TranslateString(m.Arguments[0])));
                    break;
                case nameof(DateTime.AddMonths):
                    Write(Syntax.DateTime.AddMonths(TranslateString(m.Object), TranslateString(m.Arguments[0])));
                    break;
                case nameof(DateTime.AddYears):
                    Write(Syntax.DateTime.AddYears(TranslateString(m.Object), TranslateString(m.Arguments[0])));
                    break;
                case "op_Subtract":
                    if (m.Arguments[1].Type.GetNonNullableType() == typeof(DateTime))
                    {
                        Write(Syntax.DateTime.DiffDays(TranslateString(m.Arguments[0]), TranslateString(m.Arguments[1])));
                    }
                    break;
                case nameof(DateTime.Parse):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), m.Method.DeclaringType.GetDbType()));
                    break;
                case nameof(DateTime.ToShortTimeString):
                    Write(Syntax.DateTime.ShortTime(TranslateString(m.Object)));
                    break;
                case nameof(DateTime.ToShortDateString):
                    Write(Syntax.DateTime.ShortDate(TranslateString(m.Object)));
                    break;
            }
            return m;
        }

        protected virtual Expression VisitDecimalMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case nameof(decimal.Remainder):
                    Write(Syntax.Math.Modulo(TranslateString(m.Arguments[0]), TranslateString(m.Arguments[1])));
                    break;
                case nameof(decimal.Add):
                case nameof(decimal.Subtract):
                case nameof(decimal.Multiply):
                case nameof(decimal.Divide):
                    Write("(");
                    VisitValue(m.Arguments[0]);
                    Write(" ");
                    Write(GetOperator(m.Method.Name));
                    Write(" ");
                    VisitValue(m.Arguments[1]);
                    Write(")");
                    break;
                case nameof(decimal.Negate):
                    Write(Syntax.Math.Negate(TranslateString(m.Arguments[0])));
                    Write("");
                    break;
                case nameof(decimal.Ceiling):
                    Write(Syntax.Math.Ceiling(TranslateString(m.Arguments[0])));
                    break;
                case nameof(decimal.Floor):
                    Write(Syntax.Math.Floor(TranslateString(m.Arguments[0])));
                    break;
                case nameof(decimal.Round):
                    if (m.Arguments.Count == 1)
                    {
                        Write(Syntax.Math.Round(TranslateString(m.Arguments[0])));
                    }
                    else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                    {
                        Write(Syntax.Math.Round(TranslateString(m.Arguments[0]), TranslateString(m.Arguments[1])));
                    }

                    break;
                case nameof(decimal.Truncate):
                    Write(Syntax.Math.Truncate(TranslateString(m.Arguments[0])));
                    break;
                case nameof(decimal.Parse):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), m.Method.DeclaringType.GetDbType()));
                    break;
            }

            return m;
        }

        protected virtual Expression VisitMathMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case nameof(Math.Abs):
                    Write(Syntax.Math.Abs(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Acos):
                    Write(Syntax.Math.Acos(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Asin):
                    Write(Syntax.Math.Asin(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Atan):
                    Write(Syntax.Math.Atan(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Cos):
                    Write(Syntax.Math.Cos(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Exp):
                    Write(Syntax.Math.Exp(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Log):
                    Write(Syntax.Math.Log(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Log10):
                    Write(Syntax.Math.Log10(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Sin):
                    Write(Syntax.Math.Sin(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Tan):
                    Write(Syntax.Math.Tan(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Sqrt):
                    Write(Syntax.Math.Sqrt(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Sign):
                    Write(Syntax.Math.Sign(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Ceiling):
                    Write(Syntax.Math.Ceiling(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Floor):
                    Write(Syntax.Math.Floor(TranslateString(m.Arguments[0])));
                    break;
                case nameof(Math.Pow):
                    Write(Syntax.Math.Power(TranslateString(m.Arguments[0]), TranslateString(m.Arguments[1])));
                    break;
                case nameof(Math.Round):
                    if (m.Arguments.Count == 1)
                    {
                        Write(Syntax.Math.Round(TranslateString(m.Arguments[0])));
                    }
                    else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                    {
                        Write(Syntax.Math.Round(TranslateString(m.Arguments[0]), TranslateString(m.Arguments[1])));
                    }

                    break;
                case nameof(Math.Truncate):
                    Write(Syntax.Math.Truncate(TranslateString(m.Arguments[0])));
                    break;
            }

            return m;
        }

        protected virtual Expression VisitConvertMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case nameof(Convert.ChangeType):
                    var consExp = m.Arguments[1] as ConstantExpression;
                    if (consExp != null)
                    {
                        var typeCode = DbType.String;
                        if (consExp.Value is Type)
                        {
                            typeCode = (consExp.Value as Type).GetDbType();
                        }
                        else if (consExp.Value is DbType)
                        {
                            typeCode = (DbType)consExp.Value;
                        }

                        Write(Syntax.Convert(TranslateString(m.Arguments[0]), typeCode));

                        return m;
                    }

                    break;
                case nameof(Convert.ToChar):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.StringFixedLength));
                    break;
                case nameof(Convert.ToString):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.String));
                    break;
                case nameof(Convert.ToInt16):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Int16));
                    break;
                case nameof(Convert.ToUInt16):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.UInt16));
                    break;
                case nameof(Convert.ToInt32):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Int32));
                    break;
                case nameof(Convert.ToUInt32):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.UInt32));
                    break;
                case nameof(Convert.ToInt64):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Int64));
                    break;
                case nameof(Convert.ToUInt64):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.UInt64));
                    break;
                case nameof(Convert.ToByte):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Byte));
                    break;
                case nameof(Convert.ToSByte):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.SByte));
                    break;
                case nameof(Convert.ToDecimal):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Decimal));
                    break;
                case nameof(Convert.ToSingle):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Single));
                    break;
                case nameof(Convert.ToDouble):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Double));
                    break;
                case nameof(Convert.ToBoolean):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.Boolean));
                    break;
                case nameof(Convert.ToDateTime):
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), DbType.DateTime));
                    break;
            }

            return m;
        }

        protected virtual Expression VisitRegexMethod(MethodCallExpression m)
        {
            if (m.Method.Name == nameof(Regex.IsMatch))
            {
                Write(Syntax.String.IsMatch(TranslateString(m.Arguments[0]), TranslateString(m.Arguments[1])));
            }

            return m;
        }

        protected virtual Expression VisitOtherMethod(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "ToString":
                    if (m.Object.Type == typeof(string))
                    {
                        Visit(m.Object);
                    }
                    else
                    {
                        Write(Syntax.Convert(TranslateString(m.Object), DbType.String));
                    }

                    break;
                case "Equals":
                    if (m.Method.IsStatic && m.Method.DeclaringType == typeof(object))
                    {
                        Write("(");
                        Visit(m.Arguments[0]);
                        Write(" = ");
                        Visit(m.Arguments[1]);
                        Write(")");
                    }
                    else if (!m.Method.IsStatic && m.Arguments.Count == 1 && m.Arguments[0].Type == m.Object.Type)
                    {
                        Write("(");
                        Visit(m.Object);
                        Write(" = ");
                        Visit(m.Arguments[0]);
                        Write(")");
                    }

                    break;
                case "Parse":
                    Write(Syntax.Convert(TranslateString(m.Arguments[0]), m.Method.DeclaringType.GetDbType()));
                    break;
                case "Contains":
                    Write(Syntax.String.Concat("','", TranslateString(m.Arguments[0]), "','"));
                    Write(" LIKE ");
                    var exp = m.Arguments[1];
                    if (exp is ConstantExpression constExp && constExp.Type != typeof(string) && constExp.Value != null)
                    {
                        exp = Expression.Constant(constExp.Value.ToString(), typeof(string));
                    }

                    Write(Syntax.String.Concat("'%,'", TranslateString(exp),  "',%'"));
                    break;
            }

            return m;
        }

        protected virtual Expression VisitStringMember(MemberExpression m)
        {
            switch (m.Member.Name)
            {
                case nameof(string.Length):
                    Write(Syntax.String.Length(TranslateString(m.Expression)));
                    break;
            }

            return m;
        }

        protected virtual Expression VisitDateTimeMember(MemberExpression m)
        {
            switch (m.Member.Name)
            {
                case nameof(DateTime.Day):
                    Write(Syntax.DateTime.Day(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.Month):
                    Write(Syntax.DateTime.Month(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.Year):
                    Write(Syntax.DateTime.Year(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.Hour):
                    Write(Syntax.DateTime.Hour(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.Minute):
                    Write(Syntax.DateTime.Minute(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.Second):
                    Write(Syntax.DateTime.Second(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.Millisecond):
                    Write(Syntax.DateTime.Millisecond(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.DayOfWeek):
                    Write(Syntax.DateTime.DayOfWeek(TranslateString(m.Expression)));
                    break;
                case nameof(DateTime.DayOfYear):
                    Write(Syntax.DateTime.DayOfYear(TranslateString(m.Expression)));
                    break;
            }

            return m;
        }

        protected virtual Expression VisitTimeSpanMember(MemberExpression m)
        {
            BinaryExpression bin = null;
            if (m.Expression.NodeType == ExpressionType.Subtract)
            {
                bin = m.Expression as BinaryExpression;
            }

            if (m.Expression.NodeType == ExpressionType.Convert)
            {
                bin = ((UnaryExpression)m.Expression).Operand as BinaryExpression;
            }

            if (bin == null)
            {
                return m;
            }

            switch (m.Member.Name)
            {
                case nameof(TimeSpan.TotalDays):
                case nameof(TimeSpan.Days):
                    Write(Syntax.DateTime.DiffDays(TranslateString(bin.Right), TranslateString(bin.Left)));
                    break;
                case nameof(TimeSpan.TotalHours):
                case nameof(TimeSpan.Hours):
                    Write(Syntax.DateTime.DiffHours(TranslateString(bin.Right), TranslateString(bin.Left)));
                    break;
                case nameof(TimeSpan.TotalMinutes):
                case nameof(TimeSpan.Minutes):
                    Write(Syntax.DateTime.DiffMinutes(TranslateString(bin.Right), TranslateString(bin.Left)));
                    break;
                case nameof(TimeSpan.TotalSeconds):
                case nameof(TimeSpan.Seconds):
                    Write(Syntax.DateTime.DiffSeconds(TranslateString(bin.Right), TranslateString(bin.Left)));
                    break;
            }

            return m;
        }

        protected virtual Expression VisitBinaryCoalesce(BinaryExpression b)
        {
            var left = b.Left;
            var right = b.Right;
            var array = new List<string>();

            while (right.NodeType == ExpressionType.Coalesce)
            {
                var rb = (BinaryExpression)right;
                array.Add(TranslateString(rb.Left));
                right = rb.Right;
            }

            array.Add(TranslateString(right));

            Write(Syntax.Coalesce(TranslateString(left), array.ToArray()));

            return b;
        }

        protected virtual Expression VisitBinaryPower(BinaryExpression b)
        {
            Write(Syntax.Math.Power(TranslateString(b.Left), TranslateString(b.Right)));

            return b;
        }

        protected virtual Expression VisitBinaryLogic(BinaryExpression b)
        {
            if (IsBoolean(b.Left.Type))
            {
                VisitPredicate(b.Left);
                Write(" ");
                Write(GetOperator(b));
                Write(" ");
                VisitPredicate(b.Right);
            }
            else
            {
                if (b.NodeType == ExpressionType.And ||
                    b.NodeType == ExpressionType.AndAlso)
                {
                    Write(Syntax.Math.And(TranslateString(b.Left), TranslateString(b.Right)));
                }
                else
                {
                    Write(Syntax.Math.Or(TranslateString(b.Left), TranslateString(b.Right)));
                }
            }
            return b;
        }

        protected virtual Expression VisitBinaryEqual(BinaryExpression b)
        {
            if (b.Right.NodeType == ExpressionType.Constant)
            {
                var ce = (ConstantExpression)b.Right;
                if (ce.Value == null)
                {
                    if (b.NodeType == ExpressionType.NotEqual)
                    {
                        Write(" NOT ");
                    }

                    Visit(b.Left);
                    Write(" IS NULL");

                    return null;
                }
            }
            else if (b.Left.NodeType == ExpressionType.Constant)
            {
                var ce = (ConstantExpression)b.Left;
                if (ce.Value == null)
                {
                    if (b.NodeType == ExpressionType.NotEqual)
                    {
                        Write(" NOT ");
                    }

                    Visit(b.Right);
                    Write(" IS NULL");

                    return null;
                }
            }
            return b;
        }

        protected virtual Expression VisitBinaryCompare(BinaryExpression b, ref Expression left, ref Expression right)
        {
            if (left.NodeType == ExpressionType.Call && right.NodeType == ExpressionType.Constant)
            {
                var mc = (MethodCallExpression)left;
                var ce = (ConstantExpression)right;

                if (ce.Value != null && ce.Value.GetType() == typeof(int) && ((int)ce.Value) == 0)
                {
                    if (mc.Method.Name == nameof(int.CompareTo) && !mc.Method.IsStatic && mc.Arguments.Count == 1)
                    {
                        left = mc.Object;
                        right = mc.Arguments[0];
                    }
                    else if (
                        (mc.Method.DeclaringType == typeof(string) || mc.Method.DeclaringType == typeof(decimal))
                        && mc.Method.Name == nameof(string.Compare) && mc.Method.IsStatic && mc.Arguments.Count == 2)
                    {
                        left = mc.Arguments[0];
                        right = mc.Arguments[1];
                    }
                }
            }
            return b;
        }

        protected override Expression VisitSqlText(SqlExpression sql)
        {
            Write(sql.SqlCommand);

            return sql;
        }

        protected virtual void WriteColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            if (columns.Count > 0)
            {
                for (int i = 0, n = columns.Count; i < n; i++)
                {
                    var column = columns[i];
                    if (i > 0)
                    {
                        Write(", ");
                    }

                    var c = VisitValue(column.Expression) as ColumnExpression;
                    if ((string.IsNullOrEmpty(column.Name) ||
                        (c != null && !(c is SubqueryColumnExpression) && c.Name == column.Name)))
                    {
                        continue;
                    }

                    WriteAs();
                    Write(GetColumnName(column.Name));
                }
            }
            else
            {
                Write("NULL ");

                if (IsNested)
                {
                    Write(" tmp ");
                }
            }
        }

        protected virtual void WriteAs()
        {
            Write(" AS ");
        }

        protected void WriteGroups(SelectExpression select)
        {
            if (select.GroupBy.Count > 0)
            {
                WriteLine(Indentation.Same);
                Write("GROUP BY ");

                for (int i = 0, n = select.GroupBy.Count; i < n; i++)
                {
                    if (i > 0)
                    {
                        Write(", ");
                    }

                    VisitValue(select.GroupBy[i]);
                }
            }
        }

        protected void WriteOrders(SelectExpression select)
        {
            if (select.OrderBy.Count > 0)
            {
                WriteLine(Indentation.Same);
                Write("ORDER BY ");

                for (int i = 0, n = select.OrderBy.Count; i < n; i++)
                {
                    var exp = select.OrderBy[i];
                    if (i > 0)
                    {
                        Write(", ");
                    }

                    VisitValue(exp.Expression);

                    if (exp.OrderType != OrderType.Ascending)
                    {
                        Write(" DESC");
                    }
                }
            }
        }

        protected virtual string GetTableName(TableExpression table)
        {
            var tableName = Environment == null ? table.Name : Environment.GetVariableTableName(table.Type);
            return DbUtility.FormatByQuote(Syntax, tableName);
        }

        protected virtual string GetColumnName(string columnName)
        {
            return DbUtility.FormatByQuote(Syntax, columnName);
        }

        /// <summary>
        /// 判断是否为不带参数的COUNT聚合函数。
        /// </summary>
        /// <param name="aggregateType"></param>
        /// <returns></returns>
        private bool RequiresAsteriskWhenNoArgument(AggregateType aggregateType)
        {
            return aggregateType == AggregateType.Count;
        }

        /// <summary>
        /// 切换“隐藏别名”的参数为 true。
        /// </summary>
        /// <param name="func"></param>
        protected Expression HideAliases(Func<Expression> func)
        {
            var hideTableAliases = Options.HideTableAliases;
            var hideColumnAliases = Options.HideColumnAliases;
            Options.HideTableAliases = Options.HideColumnAliases = true;

            var exp = func?.Invoke();

            Options.HideColumnAliases = hideColumnAliases;
            Options.HideTableAliases = hideTableAliases;

            return exp;
        }

        private Expression MakeJoinKey(IList<Expression> key)
        {
            if (key.Count == 1)
            {
                return key[0];
            }
            else
            {
                return Expression.New(
                    typeof(CompoundKey).GetConstructors()[0],
                    Expression.NewArrayInit(typeof(object), key.Select(k => (Expression)Expression.Convert(k, typeof(object))))
                    );
            }
        }

        class CompoundKey : IEquatable<CompoundKey>, IEnumerable<object>, IEnumerable
        {
            object[] values;
            int hc;

            public CompoundKey(params object[] values)
            {
                this.values = values;
                for (int i = 0, n = values.Length; i < n; i++)
                {
                    object value = values[i];
                    if (value != null)
                    {
                        hc ^= (value.GetHashCode() + i);
                    }
                }
            }

            public override int GetHashCode()
            {
                return hc;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public bool Equals(CompoundKey other)
            {
                if (other == null || other.values.Length != values.Length)
                    return false;
                for (int i = 0, n = other.values.Length; i < n; i++)
                {
                    if (!object.Equals(this.values[i], other.values[i]))
                        return false;
                }
                return true;
            }

            public IEnumerator<object> GetEnumerator()
            {
                return ((IEnumerable<object>)values).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
        #endregion
    }
}
