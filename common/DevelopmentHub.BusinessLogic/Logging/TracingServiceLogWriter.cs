namespace DevelopmentHub.BusinessLogic.Logging
{
    using System;
    using System.Globalization;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Logs messages using the tracing service.
    /// </summary>
    public class TracingServiceLogWriter : ILogWriter
    {
        private readonly ITracingService tracingService;
        private readonly bool addTimestamps;

        /// <summary>
        /// Initializes a new instance of the <see cref="TracingServiceLogWriter"/> class.
        /// </summary>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="timestamp">Whether or not to timestamp messages.</param>
        public TracingServiceLogWriter(ITracingService tracingService, bool timestamp = false)
        {
            this.tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            this.addTimestamps = timestamp;
        }

        /// <inheritdoc />
        public void Log(Severity severity, string tag, string message)
        {
            this.tracingService.Trace(this.FormatMessage(severity, tag, message));
        }

        private string FormatMessage(Severity severity, string tag, string message)
        {
            return $"{(this.addTimestamps ? $"{DateTime.UtcNow.ToString("o", CultureInfo.CurrentCulture)}: " : string.Empty)}{tag}: {severity}: {message}";
        }
    }
}
