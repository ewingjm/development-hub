namespace Capgemini.DevelopmentHub.Tests.Unit
{
    using System;
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;
    using Moq;

    /// <summary>
    /// Base class for workflow activity tests.
    /// </summary>
    /// <typeparam name="TCodeActivity">The type of workflow activity.</typeparam>
    public abstract class WorkflowActivityTests<TCodeActivity>
        where TCodeActivity : CodeActivity, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivityTests{TCodeActivity}"/> class.
        /// </summary>
        public WorkflowActivityTests()
        {
            this.WorkflowActivity = new TCodeActivity();
            this.WorkflowInvoker = new WorkflowInvoker(this.WorkflowActivity);

            this.TracingSvcMock = new Mock<ITracingService>();
            this.OrgSvcFactoryMock = new Mock<IOrganizationServiceFactory>();
            this.OrgSvcMock = new Mock<IOrganizationService>();
            this.WorkflowContextMock = new Mock<IWorkflowContext>();

            this.OrgSvcFactoryMock.SetReturnsDefault(this.OrgSvcMock.Object);
            this.WorkflowContextMock.Setup(workflowContext => workflowContext.UserId).Returns(Guid.NewGuid());
            this.WorkflowContextMock.Setup(workflowContext => workflowContext.SharedVariables).Returns(new ParameterCollection());

            this.WorkflowInvoker.Extensions.Add(this.WorkflowContextMock.Object);
            this.WorkflowInvoker.Extensions.Add(this.TracingSvcMock.Object);
            this.WorkflowInvoker.Extensions.Add(this.OrgSvcFactoryMock.Object);
            this.WorkflowInvoker.Extensions.Add(this.OrgSvcMock.Object);
        }

        /// <summary>
        /// Gets the workflow invoker.
        /// </summary>
        protected WorkflowInvoker WorkflowInvoker { get; }

        /// <summary>
        /// Gets the workflow activity under test.
        /// </summary>
        protected TCodeActivity WorkflowActivity { get; }

        /// <summary>
        /// Gets mock workflow context.
        /// </summary>
        protected Mock<IWorkflowContext> WorkflowContextMock { get; }

        /// <summary>
        /// Gets the mocked tracing service.
        /// </summary>
        protected Mock<ITracingService> TracingSvcMock { get; }

        /// <summary>
        /// Gets the mocked organization service factory.
        /// </summary>
        protected Mock<IOrganizationServiceFactory> OrgSvcFactoryMock { get; }

        /// <summary>
        /// Gets the mocked organization service.
        /// </summary>
        protected Mock<IOrganizationService> OrgSvcMock { get; }
    }
}
