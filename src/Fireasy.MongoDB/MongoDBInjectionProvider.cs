// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity;
using Fireasy.Data.Provider;
using MongoDB.Bson;

namespace Fireasy.MongoDB
{
    /// <summary>
    /// 实体类型的注入器。向实体类型中增加 _id 主键属性。
    /// </summary>
    public class MongoDBInjectionProvider : IInjectionProvider
    {
        IProvider IProviderService.Provider { get; set; }

        void IInjectionProvider.Inject(EntityInjectionContext context)
        {
            context.TypeBuilder.ImplementInterface(typeof(IMongoDBEntity));
            var pb = context.TypeBuilder.DefineProperty("_id", typeof(ObjectId));
            pb.DefineGetSetMethods();
        }
    }
}
