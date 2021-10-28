using System.Threading.Tasks;

namespace Fireasy.Clound.SMS
{
    public interface ISmsProvider
    {
        void Send(string[] mobiles, string content);

        Task SendAsync(string[] mobiles, string content);

        void SendWithTemplate(string[] mobiles, string template);

        Task SendWithTemplateAsync(string[] mobiles, string template);
    }
}
