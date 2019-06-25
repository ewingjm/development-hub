namespace Capgemini.DevelopmentHub.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An error returned by an OData request.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ODataError
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [DataMember(Name = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the inner error.
        /// </summary>
        [DataMember(Name = "innererror")]
        public ODataInnerError InnerError { get; set; }
    }
}
