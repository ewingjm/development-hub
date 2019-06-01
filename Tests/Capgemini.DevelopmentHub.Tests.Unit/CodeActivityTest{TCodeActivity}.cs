namespace Capgemini.DevelopmentHub.Tests.Unit
{
    using System.Activities;
    using System.Diagnostics.CodeAnalysis;
    using FakeXrmEasy;

    /// <summary>
    /// Base class for code activity unit tests.
    /// </summary>
    /// <typeparam name="TCodeActivity">The code activity under test.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class CodeActivityTest<TCodeActivity>
        where TCodeActivity : CodeActivity, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeActivityTest{TCodeActivity}"/> class.
        /// </summary>
        public CodeActivityTest()
        {
            this.FakedContext = new XrmFakedContext();
        }

        /// <summary>
        /// Gets the faked context.
        /// </summary>
        protected XrmFakedContext FakedContext { get; }
    }
}
