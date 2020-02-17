namespace DevelopmentHub.Develop.CodeActivities
{
    using System.Activities;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Extensions;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Creates a <see cref="Annotation"/> with an attachment.
    /// </summary>
    [CrmPluginRegistration(
        nameof(AttachFileToNote),
        "Attach file to note",
        "Attaches a file to a note",
        "DevelopmentHub.Develop",
        IsolationModeEnum.Sandbox)]
    public class AttachFileToNote : WorkflowActivity
    {
        /// <summary>
        /// Gets or sets the note to attach the file to.
        /// </summary>
        [Input("Note")]
        [ReferenceTarget(Annotation.EntityLogicalName)]
        [RequiredArgument]
        public InArgument<EntityReference> Note { get; set; }

        /// <summary>
        /// Gets or sets a Base64 encoded string representing the file to be attached.
        /// </summary>
        [Input("Attachment")]
        [RequiredArgument]
        public InArgument<string> Attachment { get; set; }

        /// <summary>
        /// Gets or sets the name of the file to attach.
        /// </summary>
        [Input("File name")]
        [RequiredArgument]
        public InArgument<string> FileName { get; set; }

        /// <summary>
        /// Gets or sets attachment MIME type.
        /// </summary>
        [Input("MIME type")]
        [RequiredArgument]
        public InArgument<string> MimeType { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var documentBody = this.Attachment.GetRequired(context, nameof(this.Attachment));
            logWriter.Log(Severity.Info, nameof(AttachFileToNote), $"{documentBody}");
            repoFactory.GetRepository<DevelopContext, Annotation>().Update(new Annotation
            {
                FileName = this.FileName.GetRequired(context, nameof(this.FileName)),
                DocumentBody = this.Attachment.GetRequired(context, nameof(this.Attachment)),
                IsDocument = true,
                MimeType = this.MimeType.Get(context),
                Id = this.Note.GetRequired(context, nameof(this.Note)).Id,
            });
        }
    }
}
