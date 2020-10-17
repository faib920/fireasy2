using System;

namespace Aliyun.MQ.Runtime
{
    public class ImmutableCredentials
    {
        #region Properties

        public string AccessKey { get; private set; }

        public string SecretKey { get; private set; }

		public string SecurityToken { get; private set;}

        #endregion


        #region Constructors

        public ImmutableCredentials(string accessKeyId, string secretAccessKey, string stsToken)
        {
            if (string.IsNullOrEmpty(accessKeyId)) throw new ArgumentNullException("accessKeyId");
            if (string.IsNullOrEmpty(secretAccessKey)) throw new ArgumentNullException("secretAccessKey");
			if (string.IsNullOrEmpty(stsToken))
			{
				SecurityToken = null;
			}
			else {
				SecurityToken = stsToken;
			}


            AccessKey = accessKeyId;
            SecretKey = secretAccessKey;
        }

        private ImmutableCredentials() { }

        #endregion


        #region Public methods

        public ImmutableCredentials Copy()
        {
			ImmutableCredentials credentials = new ImmutableCredentials
			{
				AccessKey = this.AccessKey,
				SecretKey = this.SecretKey,
				SecurityToken = this.SecurityToken,
            };
            return credentials;
        }

        #endregion
    }

    public abstract class ServiceCredentials
    {
        public abstract ImmutableCredentials GetCredentials();
    }

    public class BasicServiceCredentials : ServiceCredentials
    {
        #region Private members

        private ImmutableCredentials _credentials;

        #endregion


        #region Constructors

        public BasicServiceCredentials(string accessKey, string secretKey, string stsToken)
        {
            if (!string.IsNullOrEmpty(accessKey))
            {
				_credentials = new ImmutableCredentials(accessKey, secretKey, stsToken);
            }
        }

        #endregion


        #region Abstract class overrides

        public override ImmutableCredentials GetCredentials()
        {
            if (this._credentials == null)
                return null;

            return _credentials.Copy();
        }

        #endregion

    }
}
