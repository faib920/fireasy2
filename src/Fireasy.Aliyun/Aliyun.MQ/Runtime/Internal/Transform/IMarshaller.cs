namespace Aliyun.MQ.Runtime.Internal.Transform
{
    public interface IMarshaller<T, R>
    {
        T Marshall(R input);
    }
}
