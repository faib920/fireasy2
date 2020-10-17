using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline.Handlers
{
    public class CredentialsRetriever : GenericHandler
    {
        public CredentialsRetriever(ServiceCredentials credentials)
        {
            this.Credentials = credentials;
        }

        protected ServiceCredentials Credentials
        {
            get;
            private set;
        }

        protected override void PreInvoke(IExecutionContext executionContext)
        {
            ImmutableCredentials ic = null;
            if (Credentials != null && (Credentials is BasicServiceCredentials))
            {
                try
                {
                    ic = Credentials.GetCredentials();
                }
                finally
                {
                }
            }

            executionContext.RequestContext.ImmutableCredentials = ic;
        }
    }
}
