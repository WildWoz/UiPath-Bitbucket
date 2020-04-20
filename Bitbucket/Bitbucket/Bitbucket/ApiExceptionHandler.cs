using Newtonsoft.Json.Linq;
using Pathoschild.Http.Client;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Bitbucket
{
    class ApiExceptionHandler
    {
        public async Task ParseExceptionAsync(ApiException ex)
        {
            switch (ex.Status)
            {
                case HttpStatusCode.NotFound:
                    {
                        var responseText = await ex.Response.AsRawJsonObject();
                        var exceptionJson = responseText.GetValue("error");
                        var exceptionMessage = exceptionJson.Value<string>("message");
                        var exceptionDetail = exceptionJson.Value<string>("detail");
                        throw new Exception(string.Format("{0} - {1}: {2}", ex.Message, exceptionMessage, exceptionDetail));
                    }
                case HttpStatusCode.Unauthorized:
                    throw new Exception(string.Format("{0} - Check your username and password are correct.", ex.Message));
                case HttpStatusCode.Forbidden:
                    throw new Exception(string.Format("{0} - Check your Bitbucket App Password has the correct privileges and your account has access to the repository.", ex.Message));
                default:
                    throw ex;
            }
        }
    }
}
