namespace Capgemini.DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.Model;
    using Capgemini.DevelopmentHub.Model.Responses;

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
        public Task<ODataClientResponse> PostAsync<TRequest>(string path, TRequest request)
        {
            var payload = SerializeRequest(request);

            return this.PostAsync(path, payload);
        }

        /// <inheritdoc/>
        public Task<ODataClientResponse> DeleteAsync(string path)
        {
            return this.MakeODataRequest(webClient => webClient.UploadDataTaskAsync(path, HttpMethod.Delete.Method, Array.Empty<byte>()));
        }

        /// <inheritdoc/>
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request)
        {
            var payload = SerializeRequest(request);

            var response = await this.PostAsync(path, payload).ConfigureAwait(false);
            TResponse serializedResponse = DeserializeResponse<TResponse>(response.GetResponseBody());

            return serializedResponse;
        }

        /// <inheritdoc/>
        public Task<ODataClientResponse> PostAsync(string path, byte[] payload)
        {
            return this.MakeODataRequest((webClient) => webClient.UploadDataTaskAsync(path, payload));
        }

        /// <inheritdoc/>
        public async Task<RetrieveMultipleResponse<TEntity>> RetrieveMultipleAsync<TEntity>(string path, string filter = null, string[] fields = null, string orderBy = null, int? top = null, string expand = null)
        {
            string queryString = CreateQueryString(filter, fields, orderBy, top, expand);

            var response = await this.GetAsync($"{path}?{queryString}").ConfigureAwait(false);

            return DeserializeResponse<RetrieveMultipleResponse<TEntity>>(response.GetResponseBody());
        }

        /// <inheritdoc/>
        public async Task<TEntity> RetrieveAsync<TEntity>(string path, Guid entityId, string[] fields = null)
        {
            var queryString = CreateQueryString(fields: fields);
            var response = await this.GetAsync($"{path}({entityId})?{queryString}").ConfigureAwait(false);

            return DeserializeResponse<TEntity>(response.GetResponseBody());
        }

        /// <inheritdoc/>
        public async Task<ODataClientResponse> GetAsync(string path)
        {
            ODataClientResponse response;
            using (var webClient = this.GetWebClient())
            {
                var body = await webClient
                .DownloadDataTaskAsync(path)
                .ConfigureAwait(false);

                response = new ODataClientResponse(body, webClient.ResponseHeaders);
            }

            return response;
        }

        /// <inheritdoc/>
        public Task<ODataClientResponse> PatchAsync<TRequest>(string path, TRequest request)
        {
            var payload = SerializeRequest(request);

            return this.PatchAsync(path, payload);
        }

        /// <inheritdoc/>
        public Task<ODataClientResponse> PatchAsync(string path, byte[] payload)
        {
            return this.MakeODataRequest(webClient => webClient.UploadDataTaskAsync(path, "PATCH", payload));
        }

        private static string CreateQueryString(string filter = null, string[] fields = null, string orderBy = null, int? top = null, string expand = null)
        {
            var query = new NameValueCollection();

            if (!string.IsNullOrEmpty(filter))
            {
                query["$filter"] = filter;
            }

            if (fields != null && fields.Any())
            {
                query["$select"] = string.Join(",", fields);
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                query["$orderby"] = orderBy;
            }

            if (top.HasValue)
            {
                query["$top"] = top.Value.ToString(CultureInfo.CurrentCulture);
            }

            if (!string.IsNullOrEmpty(expand))
            {
                query["$expand"] = expand;
            }

            return string.Join("&", query.AllKeys.Select(key => $"{key}={Uri.EscapeDataString(query[key])}"));
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
                serializedResponse = (TResponse)new DataContractJsonSerializer(typeof(TResponse), new Type[] { typeof(Guid) }).ReadObject(s);
            }

            return serializedResponse;
        }

        private async Task<ODataClientResponse> MakeODataRequest(Func<WebClient, Task<byte[]>> request)
        {
            ODataClientResponse response;

            using (var webClient = this.GetWebClient())
            {
                byte[] responseBody;
                try
                {
                    responseBody = await request.Invoke(webClient).ConfigureAwait(false);
                    response = new ODataClientResponse(responseBody, webClient.ResponseHeaders);
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
