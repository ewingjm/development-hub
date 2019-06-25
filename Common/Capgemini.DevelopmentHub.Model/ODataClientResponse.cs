namespace Capgemini.DevelopmentHub.Model
{
    using System.Net;

    /// <summary>
    /// A response from the OData client.
    /// </summary>
    public class ODataClientResponse
    {
        private readonly byte[] responseBody;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClientResponse"/> class.
        /// </summary>
        /// <param name="responseBody">The body of the response.</param>
        /// <param name="responseHeaders">The headers of the response.</param>
        public ODataClientResponse(byte[] responseBody, WebHeaderCollection responseHeaders)
        {
            this.responseBody = responseBody;
            this.ResponseHeaders = responseHeaders;
        }

        /// <summary>
        /// Gets the response headers.
        /// </summary>
        public WebHeaderCollection ResponseHeaders { get; private set; }

        /// <summary>
        /// Gets the body of the response.
        /// </summary>
        /// <returns>The response body.</returns>
        public byte[] GetResponseBody()
        {
            return this.responseBody;
        }
    }
}
