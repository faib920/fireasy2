using System;
using System.IO;

namespace Aliyun.MQ.Runtime.Internal.Transform
{
    public abstract class UnmarshallerContext : IDisposable
    {
        private bool disposed = false;
        protected IWebResponseData WebResponseData { get; set; }

        public Stream ResponseStream { get; set; }

        public IWebResponseData ResponseData
        {
            get { return WebResponseData; }
        }

        #region Dispose Pattern Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.ResponseStream != null)
                    {
                        ResponseStream.Dispose();
                        ResponseStream = null;
                    }
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public class XmlUnmarshallerContext : UnmarshallerContext
    {

        #region Constructors

        public XmlUnmarshallerContext(Stream responseStream, IWebResponseData responseData)
        {
            this.ResponseStream = responseStream;
            this.WebResponseData = responseData;
        }

        #endregion
    }
}
