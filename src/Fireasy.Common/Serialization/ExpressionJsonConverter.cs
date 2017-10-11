// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Fireasy.Common.Serialization
{
    public class ExpressionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return typeof(Expression).IsAssignableFrom(type);
        }

        public override string WriteJson(JsonSerializer serializer, object obj)
        {
            var expression = obj as Expression;
            return new ExpressionJsonWriter(serializer, expression).ToString();
        }

        public override object ReadJson(JsonSerializer serializer, Type dataType, string json)
        {
            return new ExpressionJsonReader(serializer, json).GetExpression();
        }
    }
}
