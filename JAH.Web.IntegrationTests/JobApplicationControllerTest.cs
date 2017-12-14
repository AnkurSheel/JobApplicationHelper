using System;
using System.Net.Http;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.DomainModels;
using Xunit;
using Xunit.Abstractions;

namespace JAH.Web.IntegrationTests
{
    public class JobApplicationControllerTest : IClassFixture<ClientFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly ClientFixture _fixture;

        public JobApplicationControllerTest(ITestOutputHelper output, ClientFixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAsync_MultipleApplications_HtmlView()
        {
            // Arrange
            _fixture.JobApplicationDbContext.JobApplications.Add(new JobApplicationEntity
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                CurrentStatus = Status.None
            });
            _fixture.JobApplicationDbContext.JobApplications.Add(new JobApplicationEntity
            {
                CompanyName = "Company 2",
                ApplicationDate = new DateTime(2017, 11, 14),
                CurrentStatus = Status.Applied
            });
            _fixture.JobApplicationDbContext.JobApplications.Add(new JobApplicationEntity
            {
                CompanyName = "Company 3",
                ApplicationDate = new DateTime(2017, 11, 14),
                CurrentStatus = Status.Offer
            });
            _fixture.JobApplicationDbContext.SaveChanges();

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync("/jobApplication");

            // Assert
            response.EnsureSuccessStatusCode();
            string responseData = response.Content.ReadAsStringAsync().Result;
            _output.WriteLine(responseData);
            Assert.NotEmpty(responseData);
        }

        [Fact]
        public async Task GetAsync_NoApplications_HtmlView()
        {
            // Arrange

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync("/jobApplication");

            // Assert
            response.EnsureSuccessStatusCode();
            string responseData = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(responseData);
        }
    }
}
