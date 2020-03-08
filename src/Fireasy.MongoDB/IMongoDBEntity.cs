// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity;
using MongoDB.Bson;

namespace Fireasy.MongoDB
{
    public interface IMongoDBEntity : IEntity
    {
        ObjectId _id { get; set; }
    }
}
