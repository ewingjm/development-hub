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
        /// Get an OAuth access token using the password grant.
        /// </summary>
        /// <param name="request">The password grant request.</param>
        /// <returns>An OAuth token./returns>.
        Task<OAuthToken> GetAccessToken(OAuthPasswordGrantRequest request);
    }
}
