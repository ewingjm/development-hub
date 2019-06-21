namespace Capgemini.DevelopmentHub.Develop.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for an OData client.
    /// </summary>
    public interface IODataClient
    {
        /// <summary>
        /// Post a request to the web API.
        /// </summary>
        /// <typeparam name="TRequest">A data contract describing the request payload.</typeparam>
        /// <param name="path">The path to post the request to.</param>
        /// <param name="request">The request payload.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task PostAsync<TRequest>(string path, TRequest request);

        /// <summary>
        /// Post a request to the web API and returns the response.
        /// </summary>
        /// <typeparam name="TRequest">A data contract describing the request payload.</typeparam>
        /// <typeparam name="TResponse">A data contract describing the response.</typeparam>
        /// <param name="path">The path to post the request to.</param>
        /// <param name="request">The request payload.</param>
        /// <returns>The deserialized response.</returns>
        Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request);

        /// <summary>
        /// make a POST request to the web API.
        /// </summary>
        /// <param name="path">The path to post to.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>The response.</returns>
        Task<byte[]> PostAsync(string path, byte[] payload);

        /// <summary>
        /// Retrieve multiple records.
        /// </summary>
        /// <typeparam name="TEntity">A data contract describing the entity.</typeparam>
        /// <param name="path">The path.</param>
        /// <param name="filter">The filter string.</param>
        /// <param name="selector">The select string.</param>
        /// <param name="orderBy">The order by string.</param>
        /// <param name="top">The number of records to take.</param>
        /// <param name="expand">The expand string.</param>
        /// <returns>The entities returned by the query.</returns>
        Task<IEnumerable<TEntity>> RetrieveMultipleAsync<TEntity>(string path, string filter = "", string selector = "", string orderBy = "", int? top = null, string expand = "");

        /// <summary>
        /// Retrieve a single record.
        /// </summary>
        /// <typeparam name="TEntity">A data contract describing the entity.</typeparam>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TEntity> RetrieveAsync<TEntity>(string path);

        /// <summary>
        /// Make a GET request to the web API.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The response.</returns>
        Task<byte[]> GetAsync(string path);
    }
}
