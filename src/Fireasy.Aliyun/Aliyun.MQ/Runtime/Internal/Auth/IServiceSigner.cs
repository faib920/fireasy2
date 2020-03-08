using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Internal.Auth
{
    public partial interface IServiceSigner
    {
         void Sign(IRequest request, string accessKeyId, string secretAccessKey, string stsToken);
    }
}
