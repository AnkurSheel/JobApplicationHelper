using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.Api.Controllers;
using JobApplicationHelper.DomainModels;
using JobApplicationHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace JobApplicationHelper.Api.UnitTests
{
    [TestFixture]
    public class JobApplicationApiControllerTest
    {
        private JobApplicationApiController _jobApplicationController;
        private IJobApplicationService _jobApplicationService;

        [SetUp]
        public void Init()
        {
            _jobApplicationService = Substitute.For<IJobApplicationService>();
            _jobApplicationController = new JobApplicationApiController(_jobApplicationService);
        }

        [Test]
        public async Task ShouldReturnOkObjectResultWithAListOfJobApplications()
        {
            // Arrange
            var expectedjobApplications = new EnumerableQuery<JobApplication>(new[]
                                                                              {
                                                                                  new JobApplication { Name = "Company 1" },
                                                                                  new JobApplication { Name = "Company 2" },
                                                                                  new JobApplication { Name = "Company 3" }
                                                                              });

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);

            // Act
            var result = await _jobApplicationController.List();

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(expectedjobApplications, okResult.Value);
        }
    }
}
