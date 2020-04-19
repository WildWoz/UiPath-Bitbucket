using System.Threading.Tasks;
using Pathoschild.Http.Client;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Net.Http;

namespace Bitbucket
{
    class AsyncRequests
    {
        public static async Task<JObject> GetRequest(FluentClient client, string uri, CancellationToken cancellationToken)
        {
            return await client
                            .GetAsync(uri)
                            .WithCancellationToken(cancellationToken)
                            .AsRawJsonObject();
        }
        public static async Task<JObject> PostRequest_NoBody(FluentClient client, string uri, CancellationToken cancellationToken)
        {
            return await client
                            .PostAsync(uri)
                            .WithCancellationToken(cancellationToken)
                            .AsRawJsonObject();
        }
        public static async Task<JObject> PostRequest_WithBody(FluentClient client, string uri, CancellationToken cancellationToken, MultipartFormDataContent multipartContent)
        {
            var responseRaw = await client
                            .PostAsync(uri)
                            .WithCancellationToken(cancellationToken)
                            .WithBody(multipartContent)
                            .AsString();
            
            var response = new JObject();
            if (responseRaw == "")
                {
                    response = JObject.Parse("{\"response\":\"Commit successful\"}");
                }
            else
                {
                    response = JObject.Parse(responseRaw);
                }
            return response;
        }
        public static async Task<JObject> DeleteRequest(FluentClient client, string uri, CancellationToken cancellationToken)
        {
            var responseRaw = await client
                                .DeleteAsync(uri)
                                .WithCancellationToken(cancellationToken);
            var response = JObject.Parse("{\"response\":\"Deletion successful\"}");
            return response;
        }
    }
}
