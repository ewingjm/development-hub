namespace Capgemini.DevelopmentHub.Develop.Model
{
    using System.Runtime.Serialization;

    /// <summary>
    /// An OData error response.
    /// </summary>
    [DataContract]
    public class ODataErrorResponse
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        [DataMember(Name = "error")]
        public ODataError Error { get; set; }
    }
}
