namespace DevelopmentHub.Develop.CodeActivities
{
    using System.Activities;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Adds two whole numbers together.
    /// </summary>
    [CrmPluginRegistration(
        nameof(AddWholeNumbers),
        "Add whole numbers",
        "Adds two whole numbers together",
        "DevelopmentHub.Develop",
        IsolationModeEnum.Sandbox)]
    public class AddWholeNumbers : WorkflowActivity
    {
        /// <summary>
        /// Gets or sets the first number to add.
        /// </summary>
        [Input("Left")]
        [RequiredArgument]
        public InArgument<int> Left { get; set; }

        /// <summary>
        /// Gets or sets the second number to add.
        /// </summary>
        [Input("Right")]
        [RequiredArgument]
        public InArgument<int> Right { get; set; }

        /// <summary>
        /// Gets or sets the result of the add.
        /// </summary>
        [Output("Result")]
        public OutArgument<int> Result { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            this.Result.Set(context, this.Left.Get(context) + this.Right.Get(context));
        }
    }
}
