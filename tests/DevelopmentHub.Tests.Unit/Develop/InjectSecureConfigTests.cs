namespace DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using DevelopmentHub.Develop.Plugins;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="InjectSecureConfig"/> plugin.
    /// </summary>
    [Trait("Solution", "devhub_DevelopmentHub_Develop")]
    public class InjectSecureConfigTests : PluginTests
    {
        /// <summary>
        /// Tests that an exception is thrown for steps that have an empty secure configuration.
        /// </summary>
        [Fact]
        public void InjectSecureConfig_NoSecureConfiguration_Throws()
        {
            Assert.Throws<Exception>(() =>
            {
                this.Execute(new InjectSecureConfig(null, null));
            });
        }

        /// <summary>
        /// Tests that the secure configuration is injected into the shared variables with the correct key.
        /// </summary>
        [Fact]
        public void InjectSecureConfig_SecureConfig_AddedToSharedVariables()
        {
            var secureConfig = "this is my secure configuration";

            this.Execute(new InjectSecureConfig(string.Empty, secureConfig));

            Assert.Equal(secureConfig, this.PluginExecutionContextMock.Object.SharedVariables[InjectSecureConfig.SharedVariablesKeySecureConfig]);
        }
    }
}
