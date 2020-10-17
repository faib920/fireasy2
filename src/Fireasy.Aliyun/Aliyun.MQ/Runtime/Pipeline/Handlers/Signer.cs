using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline.Handlers
{
    public class Signer : GenericHandler
    {
        protected override void PreInvoke(IExecutionContext executionContext)
        {
            if (ShouldSign(executionContext.RequestContext))
            {
                SignRequest(executionContext.RequestContext);
                executionContext.RequestContext.IsSigned = true;
            } 
        }

        private static bool ShouldSign(IRequestContext requestContext)
        {
            return !requestContext.IsSigned;
        }

        internal static void SignRequest(IRequestContext requestContext)
        {
            ImmutableCredentials immutableCredentials = requestContext.ImmutableCredentials;

            if (immutableCredentials == null)
                return;

            try
            {
                requestContext.Signer.Sign(requestContext.Request, immutableCredentials.AccessKey, immutableCredentials.SecretKey, immutableCredentials.SecurityToken);
            }
            finally
            {
            }
        }
    }
}
