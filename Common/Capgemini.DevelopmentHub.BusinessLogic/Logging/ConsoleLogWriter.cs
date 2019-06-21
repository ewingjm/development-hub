namespace Capgemini.DevelopmentHub.BusinessLogic.Logging
{
    using System;

    /// <summary>
    /// Console log writer.
    /// </summary>
    public class ConsoleLogWriter : ILogWriter
    {
        /// <inheritdoc/>
        public void Log(Severity severity, string tag, string message)
        {
            Console.WriteLine($"{tag}: {severity}: {message}");
        }
    }
}
