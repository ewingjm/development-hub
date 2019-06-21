namespace Capgemini.DevelopmentHub.Tests.Unit
{
    using System.Diagnostics.CodeAnalysis;
    using FakeXrmEasy;

    /// <summary>
    /// Base class for custom workflow activity and plugin unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class FakedContextTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakedContextTest"/> class.
        /// </summary>
        public FakedContextTest()
        {
            this.FakedContext = new XrmFakedContext();
        }

        /// <summary>
        /// Gets the faked context.
        /// </summary>
        protected XrmFakedContext FakedContext { get; }
    }
}
