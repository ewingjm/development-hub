namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using Capgemini.DevelopmentHub.Develop.Plugins;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="InjectSecureConfig"/> plugin.
    /// </summary>
    public class InjectSecureConfigTests : FakedContextTest
    {
        /// <summary>
        /// Tests that an exception is thrown for steps that have an empty secure configuration.
        /// </summary>
        [Fact]
        public void InjectSecureConfig_NoSecureConfiguration_Throws()
        {
            Assert.Throws<Exception>(() =>
            {
                this.FakedContext.ExecutePluginWith(this.FakedContext.GetDefaultPluginContext(), new InjectSecureConfig(null, null));
            });
        }

        /// <summary>
        /// Tests that the secure configuration is injected into the shared variables with the correct key.
        /// </summary>
        [Fact]
        public void InjectSecureConfig_SecureConfig_AddedToSharedVariables()
        {
            var context = this.FakedContext.GetDefaultPluginContext();
            var secureConfig = "this is my secure configuration";

            this.FakedContext.ExecutePluginWith(
                context,
                new InjectSecureConfig(string.Empty, secureConfig));

            Assert.Equal(secureConfig, context.SharedVariables[InjectSecureConfig.SharedVariablesKeySecureConfig]);
        }
    }
}
