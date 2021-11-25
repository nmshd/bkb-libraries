using System.Net.Http;
using System.Threading.Tasks;

namespace Enmeshed.BuildingBlocks.Application.Abstractions.Infrastructure.Http
{
    public interface IHttpClient
    {
        Task<string> GetStringAsync(string uri, string authorizationToken = "", string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PostAsync<T>(string uri, T item, string authorizationToken = "",
            string requestId = "", string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> DeleteAsync(string uri, string authorizationToken = "", string requestId = "",
            string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PutAsync<T>(string uri, T item, string authorizationToken = "", string requestId = "",
            string authorizationMethod = "Bearer");
    }
}