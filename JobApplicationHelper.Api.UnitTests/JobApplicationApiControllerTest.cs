using System.Threading.Tasks;
using JobApplicationHelper.Api.Controllers;
using JobApplicationHelper.DomainModels;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace JobApplicationHelper.UnitTests
{
    [TestFixture]
    public class JobApplicationApiControllerTest
    {
        private JobApplicationApiController _jobApplicationController;

        [SetUp]
        public void Init()
        {
            _jobApplicationController = new JobApplicationApiController();
        }

        [Test]
        public async void TestMethod1()
        {
            // Arrange
            var expectedjobApplications = new[]
            {
                new JobApplication { Name = "Company 1" },
                new JobApplication { Name = "Company 2" },
                new JobApplication { Name = "Company 3" }
            };

            // Act
            var result = await _jobApplicationController.List();

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(expectedjobApplications, okResult.Value);
        }
    }
}
