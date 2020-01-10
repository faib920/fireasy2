// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;
using System;
using System.Data;
using System.Text;

namespace Fireasy.Data.Syntax
{
    public class FirebirdSyntax : ISyntaxProvider
    {
        IProvider IProviderService.Provider { get; set; }

        public StringSyntax String => new FirebirdStringSyntax();

        public DateTimeSyntax DateTime => new FirebirdDateTimeSyntax();

        public MathSyntax Math => new FirebirdMathSyntax();

        public string IdentitySelect
        {
            get { return string.Empty; }
        }

        public string IdentityColumn
        {
            get { return string.Empty; }
        }

        public string RowsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public string FakeSelect
        {
            get { return string.Empty; }
        }

        public string ParameterPrefix
        {
            get { return "@"; }
        }

        public string[] Quote
        {
            get { return new[] { "\"", "\"" }; }
        }

        public string Linefeed
        {
            get { throw new NotImplementedException(); }
        }

        public bool SupportDistinctInAggregates
        {
            get { throw new NotImplementedException(); }
        }

        public bool SupportSubqueryInSelectWithoutFrom
        {
            get { throw new NotImplementedException(); }
        }

        public string Segment(string commandText, IDataSegment segment)
        {
            if (segment.Start != null)
            {
                commandText = string.Format(@"{0}
ROWS {1} TO {2}",
                    commandText,
                    segment.Start, segment.End - 1);
            }
            else
            {
                commandText = string.Format(@"{0}
ROWS {1}",
                    commandText,
                    segment.Length);
            }

            return commandText;
        }

        public string Segment(CommandContext context)
        {
            return Segment(context.Command.CommandText, context.Segment);
        }

        public string Convert(object sourceExp, System.Data.DbType dbType)
        {
            throw new NotImplementedException();
        }

        public string Column(System.Data.DbType dbType, int? length = null, int? precision = null, int? scale = null)
        {
            throw new NotImplementedException();
        }

        public string Coalesce(object sourceExp, params object[] argExps)
        {
            if (argExps == null || argExps.Length == 0)
            {
                return sourceExp.ToString();
            }

            var sb = new StringBuilder();
            sb.AppendFormat("COALESCE({0}", sourceExp);
            foreach (var par in argExps)
            {
                sb.AppendFormat(", {0}", par);
            }

            sb.Append(")");

            return sb.ToString();
        }

        public string FormatParameter(string parameterName)
        {
            return string.Concat(ParameterPrefix, parameterName);
        }

        public string ExistsTable(string tableName)
        {
            throw new NotImplementedException();
        }

        public DbType CorrectDbType(DbType dbType)
        {
            throw new NotImplementedException();
        }
    }
}
