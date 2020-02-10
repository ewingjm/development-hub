namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.DevelopmentHub.Develop.CodeActivities;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="CreateSolution"/> workflow activity.
    /// </summary>
    [Trait("Solution", "cap_DevelopmentHub_Develop")]
    public class GetEnvironmentVariableTests : WorkflowActivityTests<GetEnvironmentVariable>
    {
        private readonly Mock<ICrmRepository<EnvironmentVariableDefinition>> envVarDefRepoMock;
        private readonly Mock<ICrmRepository<EnvironmentVariableValue>> envVarValueRepoMock;
        private readonly EntityReference environmentVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEnvironmentVariableTests"/> class.
        /// </summary>
        public GetEnvironmentVariableTests()
        {
            this.envVarDefRepoMock = new Mock<ICrmRepository<EnvironmentVariableDefinition>>();
            this.envVarValueRepoMock = new Mock<ICrmRepository<EnvironmentVariableValue>>();

            this.RepositoryFactoryMock
                .Setup(repoFactory => repoFactory.GetRepository<DevelopContext, EnvironmentVariableDefinition>())
                .Returns(this.envVarDefRepoMock.Object);

            this.RepositoryFactoryMock
                .Setup(repoFactory => repoFactory.GetRepository<DevelopContext, EnvironmentVariableValue>())
                .Returns(this.envVarValueRepoMock.Object);

            this.environmentVariable = new EntityReference(EnvironmentVariableDefinition.EntityLogicalName, Guid.NewGuid());
        }

        /// <summary>
        /// Passing a null environment variable will throw.
        /// </summary>
        [Fact]
        public void Execute_NullReference_Throws()
        {
            Assert.Throws<InvalidPluginExecutionException>(
                () => this.WorkflowInvoker.Invoke(this.GetInputs(null)));
        }

        /// <summary>
        /// Tests that if only the default value is set the default will be returned.
        /// </summary>
        [Fact]
        public void Execute_DefaultValueOnly_UsesDefault()
        {
            var defaultValue = "default";
            this.MockDefaultValue(defaultValue);

            var outputs = this.WorkflowInvoker.Invoke(this.GetInputs(this.environmentVariable));

            Assert.Equal(defaultValue, outputs[nameof(GetEnvironmentVariable.Value)]);
        }

        /// <summary>
        /// Tests that if the default value is null and the current value is set then current will be returned.
        /// </summary>
        [Fact]
        public void Execute_CurrentValueOnlyAndDefaultNull_UsesCurrent()
        {
            this.MockDefaultValue(null);
            var value = "value";
            this.MockValue(value);

            var outputs = this.WorkflowInvoker.Invoke(this.GetInputs(this.environmentVariable));

            Assert.Equal(value, outputs[nameof(GetEnvironmentVariable.Value)]);
        }

        /// <summary>
        /// Tests that if the default value empty null and the current value is set then current will be returned.
        /// </summary>
        [Fact]
        public void Execute_CurrentValueOnlyAndDefaultEmpty_UsesCurrent()
        {
            this.MockDefaultValue(string.Empty);
            var value = "value";
            this.MockValue(value);

            var outputs = this.WorkflowInvoker.Invoke(this.GetInputs(this.environmentVariable));

            Assert.Equal(value, outputs[nameof(GetEnvironmentVariable.Value)]);
        }

        /// <summary>
        /// Tests that if the default value is set and the current value is set then current will be returned.
        /// </summary>
        [Fact]
        public void Execute_CurrentValueAndDefaultValue_UsesCurrent()
        {
            this.MockDefaultValue("defaultValue");
            var value = "value";
            this.MockValue(value);

            var outputs = this.WorkflowInvoker.Invoke(this.GetInputs(this.environmentVariable));

            Assert.Equal(value, outputs[nameof(GetEnvironmentVariable.Value)]);
        }

        /// <summary>
        /// Tests that if no value is set an empty string will be returned.
        /// </summary>
        [Fact]
        public void Execute_NoDefaultOrCurrentValue_ReturnsEmptyString()
        {
            this.MockDefaultValue(null);
            this.MockValue(null);

            var outputs = this.WorkflowInvoker.Invoke(this.GetInputs(this.environmentVariable));

            Assert.Equal(string.Empty, outputs[nameof(GetEnvironmentVariable.Value)]);
        }

        private Dictionary<string, object> GetInputs(EntityReference envVar)
        {
            return new Dictionary<string, object>
            {
                { nameof(GetEnvironmentVariable.EnvironmentVariable), envVar },
            };
        }

        private void MockValue(string value)
        {
            this.envVarValueRepoMock.SetReturnsDefault(
                new List<EnvironmentVariableValue>
                {
                    new EnvironmentVariableValue
                    {
                        Value = value,
                    },
                }.AsQueryable());
        }

        private void MockDefaultValue(string defaultValue)
        {
            this.envVarDefRepoMock.SetReturnsDefault(
                new List<EnvironmentVariableDefinition>
                {
                    new EnvironmentVariableDefinition
                    {
                        DefaultValue = defaultValue,
                    },
                }.AsQueryable());
        }
    }
}
