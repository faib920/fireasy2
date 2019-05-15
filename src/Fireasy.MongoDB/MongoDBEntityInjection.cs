// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;
using Fireasy.Data.Entity;
using MongoDB.Bson;
using System;

namespace Fireasy.MongoDB
{
    /// <summary>
    /// 实体类型的注入器。向实体类型中增加 _id 主键属性。
    /// </summary>
    public class MongoDBEntityInjection : IEntityInjection
    {
        public void Inject(Type entityType, DynamicAssemblyBuilder assemblyBuilder, DynamicTypeBuilder typeBuilder)
        {
            var propertyBuilder = typeBuilder.DefineProperty("_id", typeof(ObjectId));
            propertyBuilder.DefineGetSetMethods();
        }
    }
}
