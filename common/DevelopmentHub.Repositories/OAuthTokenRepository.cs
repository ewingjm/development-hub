namespace DevelopmentHub.Repositories
{
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using DevelopmentHub.Model;
    using DevelopmentHub.Model.Requests;

    /// <summary>
    /// Provides OAuth tokens.
    /// </summary>
    public class OAuthTokenRepository : IOAuthTokenRepository
    {
        /// <summary>
        /// Gets an access token for a resource using client credentials.
        /// </summary>
        /// <param name="request">The password grant request parameters.</param>
        /// <returns>The OAuth token.</returns>
        public async Task<OAuthToken> GetAccessToken(OAuthClientCredentialsGrantRequest request)
        {
            byte[] response;

            using (var client = new SandboxWebClient())
            {
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");

                var tokenEndpoint = $"https://login.microsoftonline.com/{request.TenantId.ToString()}/oauth2/token";
                var data = new NameValueCollection
                {
                    { "resource", request.Resource.ToString() },
                    { "client_id", request.ClientId.ToString() },
                    { "grant_type", OAuthClientCredentialsGrantRequest.GrantType },
                    { "client_secret", request.ClientSecret },
                };

                response = await client.UploadValuesTaskAsync(tokenEndpoint, data).ConfigureAwait(false);
            }

            using (var ms = new MemoryStream(response))
            {
                return (OAuthToken)new DataContractJsonSerializer(typeof(OAuthToken)).ReadObject(ms);
            }
        }
    }
}
