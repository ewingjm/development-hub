namespace DevelopmentHub.Model.Requests
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contract for an OAuth password grant request.
    /// </summary>
    [DataContract]
    public class OAuthClientCredentialsGrantRequest
    {
        /// <summary>
        /// The OAuth grant type associated with this request.
        /// </summary>
        [IgnoreDataMember]
        public const string GrantType = "client_credentials";

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClientCredentialsGrantRequest"/> class.
        /// </summary>
        public OAuthClientCredentialsGrantRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClientCredentialsGrantRequest"/> class.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="clientSecret">The client secret.</param>
        public OAuthClientCredentialsGrantRequest(Guid clientId, Guid tenantId, Uri resource, string clientSecret)
        {
            this.ClientId = clientId;
            this.TenantId = tenantId;
            this.Resource = resource;
            this.ClientSecret = clientSecret;
        }

        /// <summary>
        /// Gets or sets the application's client ID.
        /// </summary>
        [DataMember]
        public Guid ClientId { get; set; }

        /// <summary>
        /// Gets or sets the application's tenant ID.
        /// </summary>
        [DataMember]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the resource to request access to.
        /// </summary>
        [DataMember]
        public Uri Resource { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        [DataMember]
        public string ClientSecret { get; set; }
    }
}
