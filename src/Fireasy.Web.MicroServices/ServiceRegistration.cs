using Fireasy.Common.Serialization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fireasy.Web.MicroServices
{
    public class ServiceRegistration
    {
        public string Url { get; set; }

        public async Task Initialize()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(Url + "/discover");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                new JsonSerializer().Deserialize<dynamic>(content);
            }
        }
    }
}
