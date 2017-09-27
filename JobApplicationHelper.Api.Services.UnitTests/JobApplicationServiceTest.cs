using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.DomainModels;
using JobApplicationHelper.Services.Services;
using NUnit.Framework;

namespace JobApplicationHelper.Api.Services.UnitTests
{
    [TestFixture]
    public class JobApplicationServiceTest
    {
        private readonly JobApplicationService _jobApplicationService;

        public JobApplicationServiceTest()
        {
            _jobApplicationService = new JobApplicationService();
        }

        [Test]
        public async Task TestMethod1()
        {
            var expectedJobApplications = new ReadOnlyCollection<JobApplication>(new List<JobApplication>
                                                             {
                                                                 new JobApplication { Name = "Company 1" },
                                                                 new JobApplication { Name = "Company 2" },
                                                                 new JobApplication { Name = "Company 3" }
                                                             });

            // Act
            var result = await _jobApplicationService.ReadAllAsync();

            // Assert
            Assert.AreEqual(expectedJobApplications, result);
        }
    }
}
