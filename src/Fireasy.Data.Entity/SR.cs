// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Localization;

namespace Fireasy.Data.Entity
{
    internal class SRKind
    {
        internal const string NotRelationProperty = "NotRelationProperty";
        internal const string FailInEntityRemove = "FailInEntityRemove";
        internal const string FailInEntityCreate = "FailInEntityCreate";
        internal const string FailInEntityUpdate = "FailInEntityUpdate";
        internal const string FailInEntityInsert = "FailInEntityInsert";
        internal const string FailInEntityMove = "FailInEntityMove";
        internal const string FailInEntityMoveWildly = "FailInEntityMoveWildly";
        internal const string FailInEntitySwap = "FailInEntitySwap";
        internal const string FailInEntityShiftUp = "FailInEntityShiftUp";
        internal const string FailInEntityShiftDown = "FailInEntityShiftDown";
        internal const string DisaccordArgument = "DisaccordArgument";
        internal const string WhereExistence = "WhereExistence";
        internal const string EntityTreeRequiredField = "EntityTreeRequiredField";
        internal const string FailInTranslateExpression = "FailInTranslateExpression";
        internal const string TranslatorNotSupported = "TranslatorNotSupported";
        internal const string NodeTranslateNotSupported = "NodeTranslateNotSupported";
        internal const string MemberTranslateNotSupported = "MemberTranslateNotSupported";
        internal const string MethodTranslateNotSupported = "MethodTranslateNotSupported";
        internal const string NewTranslateNotSupported = "NewTranslateNotSupported";
        internal const string UnaryTranslateNotSupported = "UnaryTranslateNotSupported";
        internal const string ValueTranslateNotSupported = "ValueTranslateNotSupported";
        internal const string ScalarTranslateNotSupported = "ScalarTranslateNotSupported";
        internal const string EntityQueryInvalid = "EntityQueryInvalid";
        internal const string UnknowAggregateType = "UnknowAggregateType";
        internal const string ExpressionNotSequence = "ExpressionNotSequence";
        internal const string NotDefinedPrimaryKey = "NotDefinedPrimaryKey";
        internal const string NotDefinedReference = "NotDefinedReference";
        internal const string PropertyNotFound = "PropertyNotFound";
        internal const string NotDefinedRelationship = "NotDefinedRelationship";
        internal const string NotDefinedRelationshipDirection = "NotDefinedRelationshipDirection";
        internal const string MetedataLengthValidate = "MetedataLengthValidate";
        internal const string NotEntityInstance = "NotEntityInstance";
        internal const string FaildInEntityTreeFieldType = "FaildInEntityTreeFieldType";
        internal const string UnableConvertComplexType = "UnableConvertComplexType";
        internal const string DisUpdatePrimaryProperty = "DisUpdatePrimaryProperty";
        internal const string NoneEntityTreeMetadata = "NoneEntityTreeMetadata";
        internal const string InvalidRegisterExpression = "InvalidRegisterExpression";
        internal const string InvalidCastPropertyValue = "InvalidCastPropertyValue";
        internal const string DeleteKeyNotNullable = "DeleteKeyNotNullable";
        internal const string MustAssignRewriterInterface = "MustAssignRewriterInterface";
        internal const string EntityInvalidate = "EntityInvalidate";
        internal const string ValidationUniqueCode = "ValidationUniqueCode";
        internal const string NotSupportBatcher = "NotSupportBatcher";
        internal const string InvalidOrderExpression = "InvalidOrderExpression";
        internal const string InvalidBatchOperation = "InvalidBatchOperation";
        internal const string MustBeNewExpression = "MustBeNewExpression";
        internal const string NoAnyField = "NoAnyField";
        internal const string MethodMustInExpression = "MethodMustInExpression";
        internal const string ClassNotImplInterface = "ClassNotImplInterface";
        internal const string RelationNameRepeated = "RelationNameRepeated";
        internal const string IDCardValideError = "IDCardValideError";
        internal const string MobileValideError = "MobileValideError";
        internal const string TelhponeValideError = "TelhponeValideError";
        internal const string WebSiteValideError = "WebSiteValideError";
        internal const string ZipCodeValideError = "ZipCodeValideError";
        internal const string EmailValideError = "EmailValideError";
        internal const string InvalidOperationAsNoTracking = "InvalidOperationAsNoTracking";
        internal const string MustAssignEntityContextInitializeContext = "MustAssignEntityContextInitializeContext";
        internal const string TreeCodeOutOfRange = "TreeCodeOutOfRange";
    }

    internal sealed class SR
    {
        private static StringResource m_resource;

        static SR()
        {
            if (m_resource == null)
            {
                m_resource = StringResource.Create("Strings", typeof(SR).Assembly);
            }
        }

        internal static string GetString(string kind, params object[] args)
        {
            return m_resource.GetString(kind, args);
        }
    }
}