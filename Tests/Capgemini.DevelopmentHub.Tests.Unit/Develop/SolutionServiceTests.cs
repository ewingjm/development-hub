namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Linq;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="SolutionService"/> class.
    /// </summary>
    [Trait("Solution", "cap_DevelopmentHub_Develop")]
    public class SolutionServiceTests
    {
        private readonly ISolutionService solutionService;
        private readonly Mock<IRepositoryFactory> repoFactoryMock;
        private readonly Mock<ICrmRepository<Solution>> solutionRepoMock;
        private readonly Mock<ICrmRepository<Publisher>> publisherRepoMock;
        private readonly Mock<ILogWriter> logWriterMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionServiceTests"/> class.
        /// </summary>
        public SolutionServiceTests()
        {
            this.repoFactoryMock = new Mock<IRepositoryFactory>();
            this.solutionRepoMock = new Mock<ICrmRepository<Solution>>();
            this.publisherRepoMock = new Mock<ICrmRepository<Publisher>>();
            this.logWriterMock = new Mock<ILogWriter>();

            this.repoFactoryMock
                .Setup(repoFactory => repoFactory.GetRepository<DevelopContext, Solution>())
                .Returns(this.solutionRepoMock.Object);
            this.repoFactoryMock
                .Setup(repoFactory => repoFactory.GetRepository<DevelopContext, Publisher>())
                .Returns(this.publisherRepoMock.Object);

            this.solutionService = new SolutionService(
                this.repoFactoryMock.Object, this.logWriterMock.Object);
        }

        /// <summary>
        /// Tests that calling create without a unique name throws an exception.
        /// </summary>
        [Fact]
        public void Create_NoUniqueName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                this.solutionService.Create(string.Empty, "display name", "description");
            });
        }

        /// <summary>
        /// Tests that calling create without a display name throws an exception.
        /// </summary>
        [Fact]
        public void Create_NoDisplayName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                this.solutionService.Create("uniquename", string.Empty, "description");
            });
        }

        /// <summary>
        /// Tests that calling create with a unique name that doesn't match an existing publisher throws a <see cref="InvalidPluginExecutionException"/>.
        /// </summary>
        [Fact]
        public void Create_NoPublisherFoundForUniqueName_ThrowsInvalidPluginExecutionException()
        {
            Assert.Throws<InvalidPluginExecutionException>(() =>
            {
                this.solutionService.Create("cap_Solution", "Solution", string.Empty);
            });
        }

        /// <summary>
        /// Tests that a solution is created with the publisher returned.
        /// </summary>
        [Fact]
        public void Create_PublisherFound_CreatesSolutionWithPublisher()
        {
            var expectedPublisher = new Publisher { PublisherId = Guid.NewGuid() };
            this.publisherRepoMock.SetReturnsDefault(new Publisher[] { expectedPublisher }.AsQueryable());
            this.solutionRepoMock.Setup((solutionRepoMock) => solutionRepoMock.Create(It.IsAny<Solution>()));

            this.solutionService.Create("cap_Solution", "Solution", string.Empty);

            this.solutionRepoMock.Verify(
                (solutionRepo) => solutionRepo.Create(
                    It.Is<Solution>(
                        (solution) => solution.PublisherId.Id == expectedPublisher.Id)));
        }

        /// <summary>
        /// Tests that calling without a unique name throws an <see cref="ArgumentException"/>.
        /// </summary>
        [Fact]
        public void GetSolutionPublisher_NoUniqueName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                this.solutionService.GetSolutionPublisher(string.Empty);
            });
        }

        /// <summary>
        /// Tests that calling without a valid unqiue name throws an <see cref="ArgumentException"/>.
        /// </summary>
        [Fact]
        public void GetSolutionPublisher_InvalidUniqueName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                this.solutionService.GetSolutionPublisher("solution");
            });
        }
    }
}
