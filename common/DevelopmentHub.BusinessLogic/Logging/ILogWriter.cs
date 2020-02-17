namespace DevelopmentHub.BusinessLogic.Logging
{
    /// <summary>
    /// Interface for a log writer.
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// Write to the log.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="tag">A tag to prepend onto the message (generally the name of the calling class).</param>
        /// <param name="message">The message to log.</param>
        void Log(Severity severity, string tag, string message);
    }
}
