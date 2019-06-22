namespace Capgemini.DevelopmentHub.Develop.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using System.Web;
    using Capgemini.DevelopmentHub.Develop.Model;

    /// <summary>
    /// OData client.
    /// </summary>
    public class ODataClient : IODataClient
    {
        private const string RequestHeaderODataVersion = "OData-Version";
        private const string RequestHeaderODataMaxVersion = "OData-MaxVersion";

        private readonly Uri api;
        private readonly OAuthToken oAuthToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClient"/> class.
        /// </summary>
        /// <param name="api">Web API URL.</param>
        /// <param name="oAuthToken">OAuth token.</param>
        public ODataClient(Uri api, OAuthToken oAuthToken)
        {
            if (api.AbsolutePath != "/api/data/v9.1/")
            {
                api = new Uri(new Uri(api.GetLeftPart(UriPartial.Authority)), "api/data/v9.1/");
            }

            this.api = api;
            this.oAuthToken = oAuthToken;
        }

        /// <inheritdoc/>
        public async Task PostAsync<TRequest>(string path, TRequest request)
        {
            var payload = SerializeRequest(request);

            await this.PostAsync(path, payload).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request)
        {
            var payload = SerializeRequest(request);

            var response = await this.PostAsync(path, payload).ConfigureAwait(false);
            TResponse serializedResponse = DeserializeResponse<TResponse>(response);

            return serializedResponse;
        }

        /// <inheritdoc/>
        public async Task<byte[]> PostAsync(string path, byte[] payload)
        {
            return await this.MakeODataRequest((webClient) => webClient.UploadDataTaskAsync(path, payload)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> RetrieveMultipleAsync<TEntity>(string path, string filter = "", string select = "", string orderBy = "", int? top = null, string expand = "")
        {
            string queryString = CreateQueryString(filter, select, orderBy, top, expand);

            var response = await this.GetAsync($"{path}?{queryString}").ConfigureAwait(false);

            return DeserializeResponse<IEnumerable<TEntity>>(response);
        }

        /// <inheritdoc/>
        public async Task<TEntity> RetrieveAsync<TEntity>(string path)
        {
            var response = await this.GetAsync(path).ConfigureAwait(false);

            return DeserializeResponse<TEntity>(response);
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetAsync(string path)
        {
            byte[] response;
            using (var webClient = this.GetWebClient())
            {
                response = await webClient
                .DownloadDataTaskAsync(path)
                .ConfigureAwait(false);
            }

            return response;
        }

        private static string CreateQueryString(string filter, string select, string orderBy, int? top, string expand)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (string.IsNullOrEmpty(filter))
            {
                query["filter"] = filter;
            }

            if (string.IsNullOrEmpty(select))
            {
                query["select"] = select;
            }

            if (string.IsNullOrEmpty(orderBy))
            {
                query["orderby"] = orderBy;
            }

            if (top.HasValue)
            {
                query["top"] = top.Value.ToString(CultureInfo.CurrentCulture);
            }

            if (string.IsNullOrEmpty(expand))
            {
                query["expand"] = expand;
            }

            return query.ToString();
        }

        private static byte[] SerializeRequest<TRequest>(TRequest request)
        {
            byte[] payload;
            using (var s = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(TRequest)).WriteObject(s, request);
                payload = s.ToArray();
            }

            return payload;
        }

        private static TResponse DeserializeResponse<TResponse>(byte[] response)
        {
            TResponse serializedResponse;
            using (var s = new MemoryStream(response))
            {
                serializedResponse = (TResponse)new DataContractJsonSerializer(typeof(TResponse)).ReadObject(s);
            }

            return serializedResponse;
        }

        private async Task<byte[]> MakeODataRequest(Func<WebClient, Task<byte[]>> request)
        {
            byte[] response;

            using (var webClient = this.GetWebClient())
            {
                try
                {
                    response = await request.Invoke(webClient).ConfigureAwait(false);
                }
                catch (WebException ex)
                {
                    var oDataError = (ODataErrorResponse)new DataContractJsonSerializer(typeof(ODataErrorResponse), new Type[] { typeof(ODataInnerError), typeof(ODataError) })
                        .ReadObject(ex.Response.GetResponseStream());

                    // It would be preferable to throw a custom ODataException but custom exceptions are not supported in the sandbox.
                    throw new WebException(oDataError.Error.Message, ex);
                }
            }

            return response;
        }

        private WebClient GetWebClient()
        {
            return new SandboxWebClient
            {
                BaseAddress = this.api.ToString(),
                Headers =
                {
                    { HttpRequestHeader.Accept, "application/json" },
                    { HttpRequestHeader.Authorization, $"Bearer {this.oAuthToken.AccessToken}" },
                    { HttpRequestHeader.ContentType, "application/json" },
                    { RequestHeaderODataVersion, "4.0" },
                    { RequestHeaderODataMaxVersion, "4.0" },
                },
            };
        }
    }
}
