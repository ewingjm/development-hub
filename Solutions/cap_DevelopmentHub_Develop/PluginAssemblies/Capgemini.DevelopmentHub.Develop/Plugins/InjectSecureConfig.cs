namespace Capgemini.DevelopmentHub.Develop.Plugins
{
    using System;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Inject secure configuration into an execution context.
    /// WARNING: this will expose the secure configuration in the execution context.
    /// </summary>
    [CrmPluginRegistration(
        "cap_ImportSolution",
        cap_environment.EntityLogicalName,
        StageEnum.PreOperation,
        ExecutionModeEnum.Synchronous,
        null,
        "Inject OAuth password grant for import solution",
        0,
        IsolationModeEnum.Sandbox)]
    public class InjectSecureConfig : Plugin
    {
        /// <summary>
        /// The key used for the secure configuration in the shared variables.
        /// </summary>
        public const string SharedVariablesKeySecureConfig = "SecureConfiguration";

        private const string Tag = nameof(InjectSecureConfig);

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectSecureConfig"/> class.
        /// </summary>
        /// <param name="unsecureConfig">Unsecure configuration.</param>
        /// <param name="secureConfig">Secure configuration.</param>
        public InjectSecureConfig(string unsecureConfig, string secureConfig)
            : base(unsecureConfig, secureConfig)
        {
        }

        /// <inheritdoc/>
        protected override void Execute(IPluginExecutionContext context, IOrganizationService orgSvc, TracingServiceLogWriter logWriter, RepositoryFactory repositoryFactory)
        {
            logWriter.Log(Severity.Info, Tag, "Injecting secure configuration into shared variables.");

            if (string.IsNullOrEmpty(this.SecureConfig))
            {
                throw new Exception("The secure configuration for the plugin step is empty.");
            }

            context.SharedVariables.Add(SharedVariablesKeySecureConfig, this.SecureConfig);
        }
    }
}
