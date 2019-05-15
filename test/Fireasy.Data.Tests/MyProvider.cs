using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Data;

namespace Fireasy.Data.Tests
{
    public class MyProvider : ProviderBase
    {
        public override ConnectionParameter GetConnectionParameter(ConnectionString connectionString)
        {
            throw new NotImplementedException();
        }

        public override string UpdateConnectionString(ConnectionString connectionString, ConnectionParameter parameter)
        {
            throw new NotImplementedException();
        }
    }

    public class MySyntax : ISyntaxProvider
    {
        public StringSyntax String => throw new NotImplementedException();

        public DateTimeSyntax DateTime => throw new NotImplementedException();

        public MathSyntax Math => throw new NotImplementedException();

        public string IdentitySelect => throw new NotImplementedException();

        public string IdentityColumn => throw new NotImplementedException();

        public string RowsAffected => throw new NotImplementedException();

        public string FakeSelect => throw new NotImplementedException();

        public string ParameterPrefix => throw new NotImplementedException();

        public string[] Quote => throw new NotImplementedException();

        public string Linefeed => throw new NotImplementedException();

        public bool SupportDistinctInAggregates => throw new NotImplementedException();

        public bool SupportSubqueryInSelectWithoutFrom => throw new NotImplementedException();

        public IProvider Provider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Coalesce(object sourceExp, params object[] argExps)
        {
            throw new NotImplementedException();
        }

        public string Column(DbType dbType, int? length = null, int? precision = null, int? scale = null)
        {
            throw new NotImplementedException();
        }

        public string Convert(object sourceExp, DbType dbType)
        {
            throw new NotImplementedException();
        }

        public DbType CorrectDbType(DbType dbType)
        {
            throw new NotImplementedException();
        }

        public string ExistsTable(string tableName)
        {
            throw new NotImplementedException();
        }

        public string FormatParameter(string parameterName)
        {
            throw new NotImplementedException();
        }

        public string Segment(string commandText, IDataSegment segment)
        {
            throw new NotImplementedException();
        }

        public string Segment(CommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}
