namespace DevelopmentHub.Model.Requests
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contract for an OAuth password grant request.
    /// </summary>
    [DataContract]
    public class OAuthPasswordGrantRequest
    {
        /// <summary>
        /// The OAuth grant type associated with this request.
        /// </summary>
        [IgnoreDataMember]
        public const string GrantType = "password";

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthPasswordGrantRequest"/> class.
        /// </summary>
        public OAuthPasswordGrantRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthPasswordGrantRequest"/> class.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public OAuthPasswordGrantRequest(Guid clientId, Guid tenantId, Uri resource, string username, string password)
        {
            this.ClientId = clientId;
            this.TenantId = tenantId;
            this.Resource = resource;
            this.Username = username;
            this.Password = password;
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
        /// Gets or sets the username to authenticate.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password to authenticate.
        /// </summary>
        [DataMember]
        public string Password { get; set; }
    }
}
