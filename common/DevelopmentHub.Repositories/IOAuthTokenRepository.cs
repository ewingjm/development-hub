namespace DevelopmentHub.Repositories
{
    using System.Threading.Tasks;
    using DevelopmentHub.Model;
    using DevelopmentHub.Model.Requests;

    /// <summary>
    /// Interface for an OAuth token repository.
    /// </summary>
    public interface IOAuthTokenRepository
    {
        /// <summary>
        /// Gets an access token for a resource using client credentials.
        /// </summary>
        /// <param name="request">The password grant request parameters.</param>
        /// <returns>The OAuth token.</returns>
        Task<OAuthToken> GetAccessToken(OAuthClientCredentialsGrantRequest request);
    }
}
