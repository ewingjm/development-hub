namespace DevelopmentHub.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contract for an OAuth token.
    /// </summary>
    [DataContract]
    public class OAuthToken
    {
        /// <summary>
        /// Gets or sets the resource the token applies to.
        /// </summary>
        [DataMember(Name = "resource")]
        public Uri Resource { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
