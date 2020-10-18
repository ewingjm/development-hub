namespace DevelopmentHub.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception thrown by <see cref="Repositories.IODataClient"/>.
    /// </summary>
    [Serializable]
    public class ODataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/> class.
        /// </summary>
        public ODataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/> class.
        /// </summary>
        /// <param name="error">The serialized ODataError.</param>
        public ODataException(ODataError error)
            : base(error?.Message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ODataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ODataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/> class.
        /// </summary>
        /// <param name="serializationInfo">Serialization info.</param>
        /// <param name="streamingContext">Streaming context.</param>
        protected ODataException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
