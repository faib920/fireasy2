// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fireasy.Data.Entity.QueryBuilder
{
    internal class EntityQueryBuilder
    {
        private readonly StringBuilder m_builder;
        private readonly ISyntaxProvider m_syntax;
        private readonly Type m_entityType;
        private readonly EntityPersistentEnvironment m_environment;
        private readonly EntityMetadata m_metadata;
        private readonly ParameterCollection m_parameters;
        private bool m_isSelect;
        private bool m_isWhere;
        private bool m_isOrderBy;
        private QueryType m_queryType;

        internal EntityQueryBuilder(ISyntaxProvider syntax, EntityPersistentEnvironment environment, Type entityType)
        {
            m_builder = new StringBuilder();
            m_syntax = syntax;
            m_environment = environment;
            m_entityType = entityType;
            m_metadata = EntityMetadataUnity.GetEntityMetadata(m_entityType);
        }

        internal EntityQueryBuilder(ISyntaxProvider syntax, EntityPersistentEnvironment environment, Type entityType, ParameterCollection parameters)
            : this(syntax, environment, entityType)
        {
            m_parameters = parameters;
        }

        internal EntityQueryBuilder(EntityQueryContext context, Type entityType)
            : this(context.Syntax, context.Environment, entityType, context.Parameters)
        {
        }

        internal EntityQueryBuilder Select()
        {
            m_builder.Append("SELECT");
            m_queryType = QueryType.Select;
            return this;
        }

        internal EntityQueryBuilder Single(IEnumerable<IProperty> properties)
        {
            return Single(properties.ToArray());
        }

        internal EntityQueryBuilder Single(params IProperty[] properties)
        {
            var flag = new AssertFlag();
            foreach (var property in properties)
            {
                if (m_isSelect || !flag.AssertTrue())
                {
                    m_builder.Append(",");
                }
                m_builder.Append(" " + QuoteColumn(property));
            }
            m_isSelect = true;
            return this;
        }

        internal EntityQueryBuilder Single(string field)
        {
            if (m_isSelect)
            {
                m_builder.Append(",");
            }
            m_builder.Append(" " + field);
            return this;
        }

        internal EntityQueryBuilder All()
        {
            return Single(PropertyUnity.GetLoadedProperties(m_entityType).ToArray());
        }

        internal EntityQueryBuilder Count()
        {
            //取主键
            var primaryKey = PropertyUnity.GetPrimaryProperties(m_entityType).FirstOrDefault();
            if (primaryKey != null)
            {
                m_builder.AppendFormat(" COUNT({0})", QuoteColumn(primaryKey));
            }
            else
            {
                //使用第一个列
                var firstProperty = PropertyUnity.GetPersistentProperties(m_entityType).FirstOrDefault();
                m_builder.AppendFormat(" COUNT({0})", QuoteColumn(firstProperty));
            }
            return this;
        }

        internal EntityQueryBuilder From()
        {
            m_builder.AppendFormat(" FROM {0} T", QuoteTable());
            return this;
        }

        internal EntityQueryBuilder From(EntityQueryBuilder builder)
        {
            m_builder.AppendFormat(" FROM ({0}) T", builder);
            return this;
        }

        internal EntityQueryBuilder Where(string condition)
        {
            if (m_isWhere)
            {
                throw new Exception(SR.GetString(SRKind.WhereExistence));
            }
            var fakeDelete = ConditionFakeDelete();
            if (string.IsNullOrEmpty(condition))
            {
                return this;
            }
            m_builder.AppendFormat(!fakeDelete ? " WHERE {0}" : " AND {0}", condition);
            m_isWhere = true;
            return this;
        }

        internal EntityQueryBuilder Where(IProperty property, PropertyValue value, QueryOperator oper = QueryOperator.Equals)
        {
            return Where(property, value.GetStorageValue(), oper);
        }

        internal EntityQueryBuilder Where(IProperty property, object value, QueryOperator oper = QueryOperator.Equals)
        {
            var fakeDelete = ConditionFakeDelete();
            if (m_isWhere)
            {
                throw new Exception(SR.GetString(SRKind.WhereExistence));
            }
            m_builder.Append(!fakeDelete ? " WHERE " : " AND ");
            Condition(property.Info.FieldName, value, oper);
            m_isWhere = true;
            return this;
        }

        internal EntityQueryBuilder And(string condition)
        {
            if (!m_isWhere)
            {
                return Where(condition);
            }
            m_builder.AppendFormat(" AND {0}", condition);
            return this;
        }

        internal EntityQueryBuilder And(IProperty property, PropertyValue value, QueryOperator oper = QueryOperator.Equals)
        {
            return And(property, value.GetStorageValue(), oper);
        }

        internal EntityQueryBuilder And(IProperty property, object value, QueryOperator oper = QueryOperator.Equals)
        {
            if (!m_isWhere)
            {
                return Where(property, value, oper);
            }
            m_builder.Append(" AND");
            Condition(property.Info.FieldName, value, oper);
            return this;
        }

        internal EntityQueryBuilder Or(string condition)
        {
            if (!m_isWhere)
            {
                return Where(condition);
            }
            m_builder.AppendFormat(" OR {0}", condition);
            return this;
        }

        internal EntityQueryBuilder Or(IProperty property, PropertyValue value, QueryOperator oper = QueryOperator.Equals)
        {
            return Or(property, value.GetStorageValue(), oper);
        }

        internal EntityQueryBuilder Or(IProperty property, object value, QueryOperator oper = QueryOperator.Equals)
        {
            if (!m_isWhere)
            {
                return Where(property, value, oper);
            }
            m_builder.Append(" OR");
            Condition(property.Info.FieldName, value, oper);
            return this;
        }

        internal EntityQueryBuilder OrderBy(params IProperty[] orderBys)
        {
            if (orderBys == null || orderBys.Length == 0)
            {
                return this;
            }
            if (!m_isOrderBy)
            {
                m_builder.Append(" ORDER BY");
            }
            var flag = new AssertFlag();
            foreach (var property in orderBys)
            {
                if (m_isOrderBy || !flag.AssertTrue())
                {
                    m_builder.Append(",");
                }
                m_builder.Append(" " + QuoteColumn(property));
            }
            m_isOrderBy = true;
            return this;
        }

        internal EntityQueryBuilder OrderByDesc(params IProperty[] orderBys)
        {
            if (orderBys == null || orderBys.Length == 0)
            {
                return this;
            }
            if (!m_isOrderBy)
            {
                m_builder.Append(" ORDER BY");
            }
            var flag = new AssertFlag();
            foreach (var property in orderBys)
            {
                if (m_isOrderBy || !flag.AssertTrue())
                {
                    m_builder.Append(",");
                }
                m_builder.Append(" " + QuoteColumn(property) + " DESC");
            }
            m_isOrderBy = true;
            return this;
        }

        internal EntityQueryBuilder OrderBy(string orderBy)
        {
            if (string.IsNullOrEmpty(orderBy))
            {
                return this;
            }
            if (!m_isOrderBy)
            {
                m_builder.Append(" ORDER BY " + orderBy);
                m_isOrderBy = true;
            }
            else
            {
                m_builder.Append("," + orderBy);
            }
            return this;
        }

        internal EntityQueryBuilder Insert()
        {
            m_builder.AppendFormat("INSERT INTO {0}", QuoteTable());
            m_queryType = QueryType.Insert;
            return this;
        }

        internal EntityQueryBuilder Update()
        {
            m_builder.AppendFormat("UPDATE {0}", QuoteTable());
            m_queryType = QueryType.Update;
            return this;
        }

        internal EntityQueryBuilder Delete()
        {
            m_builder.AppendFormat("DELETE FROM {0}", QuoteTable());
            m_queryType = QueryType.Delete;
            return this;
        }

        internal EntityQueryBuilder Set(params QueryValue[] values)
        {
            if (m_queryType == QueryType.Insert)
            {
                return SetByInsert(values);
            }
            else if (m_queryType == QueryType.Update)
            {
                return SetByUpdate(values);
            }
            return this;
        }

        private EntityQueryBuilder SetByInsert(params QueryValue[] values)
        {
            var flag = new AssertFlag();
            m_builder.Append("(");
            var realValues = values.Where(s => s.Property is ISavedProperty);
            foreach (var value in realValues)
            {
                if (!flag.AssertTrue())
                {
                    m_builder.Append(", ");
                }
                m_builder.Append(QuoteColumn(value.Property));
            }
            m_builder.Append(") VALUES(");
            flag.Reset();
            foreach (var value in realValues)
            {
                if (!flag.AssertTrue())
                {
                    m_builder.Append(", ");
                }
                if (m_parameters == null)
                {
                    m_builder.Append(ConvertValue(value.Value));
                }
                else
                {
                    m_builder.AppendFormat("{0}p{1}", m_syntax.ParameterPrefix, m_parameters.Count);
                    var parameter = new Parameter(string.Format("p{0}", m_parameters.Count));
                    value.Value.As<PropertyValue>(v => PropertyValue.Set(v, parameter), () => parameter.Value = value.Value);
                    m_parameters.Add(parameter);
                }
            }
            m_builder.Append(")");
            return this;
        }

        private EntityQueryBuilder SetByUpdate(params QueryValue[] values)
        {
            var flag = new AssertFlag();
            m_builder.Append(" SET ");
            var realValues = values.Where(s => s.Property is ISavedProperty);
            foreach (var value in realValues)
            {
                if (!flag.AssertTrue())
                {
                    m_builder.Append(", ");
                }
                Condition(value.Property.Info.FieldName, value.Value);
            }
            return this;
        }

        public override string ToString()
        {
            return m_builder.ToString();
        }

        internal SqlCommand ToSqlCommand()
        {
            return new SqlCommand(ToString());
        }

        private string GetOperatorChar(QueryOperator @operator)
        {
            switch (@operator)
            {
                case QueryOperator.Equals: return "=";
                case QueryOperator.UnEquals: return "<>";
                case QueryOperator.LessThen: return "<";
                case QueryOperator.GreaterThan: return ">";
                case QueryOperator.LessThenOrEquals: return "<=";
                case QueryOperator.GreaterThanOrEquals: return ">=";
                case QueryOperator.Like: return "LIKE";
            }
            return string.Empty;
        }

        private string QuoteColumn(IProperty property)
        {
            var sqc = property as SubqueryProperty;
            if (sqc != null)
            {
                return "(" + sqc.Subquery.Replace("$", "T.") + ") " + DbUtility.FormatByQuote(m_syntax, sqc.Name);
            }
            return DbUtility.FormatByQuote(m_syntax, property.Info.FieldName);
        }

        private string Quote(string name)
        {
            return DbUtility.FormatByQuote(m_syntax, name);
        }

        private string QuoteTable()
        {
            var rootType = m_entityType.GetRootType();
            if (m_environment != null)
            {
                return DbUtility.FormatByQuote(m_syntax, m_environment.GetVariableTableName(rootType));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(rootType);
                return DbUtility.FormatByQuote(m_syntax, metadata.TableName);
            }
        }

        private void Condition(string field, object value, QueryOperator oper = QueryOperator.Equals)
        {
            if (m_parameters == null)
            {
                m_builder.AppendFormat(" {0} {1} {2}", Quote(field), GetOperatorChar(oper), ConvertValue(value));
            }
            else
            {
                m_builder.AppendFormat(" {0} {1} {2}p{3}", Quote(field), GetOperatorChar(oper), m_syntax.ParameterPrefix, m_parameters.Count);
                var parameter = new Parameter(string.Format("p{0}", m_parameters.Count));
                value.As<PropertyValue>(v => PropertyValue.Set(v, parameter), () => parameter.Value = value);
                m_parameters.Add(parameter);
            }
        }

        private bool ConditionFakeDelete()
        {
            if (m_metadata.DeleteProperty != null)
            {
                m_builder.AppendFormat(" WHERE {0} = 0", QuoteColumn(m_metadata.DeleteProperty));
                return true;
            }
            return false;
        }

        private object ConvertValue(object value)
        {
            if (value is string || value is DateTime)
            {
                return "'" + value + "'";
            }
            return value;
        }
    }

    internal enum QueryOperator
    {
        Equals,
        UnEquals,
        LessThen,
        GreaterThan,
        LessThenOrEquals,
        GreaterThanOrEquals,
        Like
    }

    internal enum QueryType
    {
        Select,
        Insert,
        Update,
        Delete
    }

    internal class QueryValue
    {
        public IProperty Property { get; set; }

        public object Value { get; set; }
    }
}