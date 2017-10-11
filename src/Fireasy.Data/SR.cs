// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Localization;

namespace Fireasy.Data
{
    internal class SRKind
    {
        internal const string BatcherException = "BatcherException";
        internal const string BatcherDataTable = "BatcherDataTable";
        internal const string FailInBackup = "FailInBackup";
        internal const string FailInExecute = "FailInExecute";
        internal const string FileNotFound = "FileNotFound";
        internal const string RegistryInvalid = "RegistryInvalid";
        internal const string FailInDataParse = "FailInDataParse";
        internal const string ConverterNotSupported = "ConverterNotSupported";
        internal const string NonRequiredAttribute = "NonRequiredAttribute";
        internal const string UnableCreateSequence = "UnableCreateSequence";
        internal const string AssemblyDbPfNull = "AssemblyDbPfNull";
        internal const string StandardDbPfNull = "StandardDbPfNull";
        internal const string UnknownSchemaCategory = "UnknownSchemaCategory";
        internal const string SchemaNotSupported = "SchemaNotSupported";
        internal const string SchemaGetFaild = "SchemaGetFaild";
        internal const string SchemaQueryNotSupported = "SchemaQueryNotSupported";
        internal const string SyntaxParseNotSupported = "SyntaxParseNotSupported";
        internal const string UnableInitCommand = "UnableInitCommand";
        internal const string NonInstanceConfigurationSection = "NonInstanceConfigurationSection";
        internal const string InstanceConfigurationInvalid = "InstanceConfigurationInvalid";
        internal const string ProviderNotSupported = "ProviderNotSupported";
        internal const string MoreFieldCount = "MoreFieldCount";
        internal const string UnableGetDatabaseScope = "UnableGetDatabaseScope";
        internal const string NotMappingDbType = "NotMappingDbType";
        internal const string UnableOpenConnection = "UnableOpenConnection";
        internal const string UnableCloseConnection = "UnableCloseConnection";
        internal const string ConnectionError = "ConnectionError";
        internal const string UnableCastRowMapper = "UnableCastRowMapper";
        internal const string PropertyTypeIsNull = "PropertyTypeIsNull";
    }

    internal class SR
    {
        private static StringResource sources;

        static SR()
        {
            if (sources == null)
            {
                sources = StringResource.Create("Strings", typeof(SR).Assembly);
            }
        }

        internal static string GetString(string kind, params object[] args)
        {
            return sources.GetString(kind, args);
        }
    }
}