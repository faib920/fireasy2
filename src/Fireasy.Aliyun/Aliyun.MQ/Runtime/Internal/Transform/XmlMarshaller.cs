using Aliyun.MQ.Util;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Aliyun.MQ.Runtime.Internal.Transform
{
    internal class XmlMarshaller<TRequest> : IMarshaller<Stream, TRequest>
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(TRequest));

        public Stream Marshall(TRequest requestObject)
        {
            MemoryStream stream = null;
            var gotException = false;
            try
            {
                stream = new MemoryStream();
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, Constants.MQ_XML_NAMESPACE);
                _serializer.Serialize(stream, requestObject, namespaces);
                stream.Seek(0, SeekOrigin.Begin);
            }
            catch (InvalidOperationException ex)
            {
                gotException = true;
                throw new RequestMarshallException(ex.Message, ex);
            }
            finally
            {
                if (gotException && stream != null)
                {
                    stream.Close();
                }
            }
            return stream;
        }
    }
}
