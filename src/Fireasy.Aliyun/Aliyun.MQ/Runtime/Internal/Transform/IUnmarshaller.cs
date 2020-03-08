namespace Aliyun.MQ.Runtime.Internal.Transform
{
    public interface IUnmarshaller<T, R>
    {
        T Unmarshall(R input);
    }
}
