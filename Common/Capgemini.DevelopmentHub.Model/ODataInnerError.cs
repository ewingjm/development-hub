namespace Capgemini.DevelopmentHub.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An inner error returned by an OData request.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ODataInnerError
    {
        /// <summary>
        /// Gets or sets the inner error message.
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the type of error.
        /// </summary>
        [DataMember(Name = "type")]

        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        [DataMember(Name = "stacktrace")]
        public string StackTrace { get; set; }
    }
}
