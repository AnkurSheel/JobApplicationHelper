using System;
using System.Threading.Tasks;
using Autofac;
using JAH.Data;
using JAH.Data.Entities;
using JAH.DomainModels;
using Microsoft.EntityFrameworkCore;
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
        public async Task ShouldReturnCorrectResponseWithAllJobApplications()
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

            const string expectedResponse = "[{\"name\":\"Company 1\",\"startDate\":\"2017-11-13T00:00:00\",\"status\":0}," +
                                            "{\"name\":\"Company 2\",\"startDate\":\"2017-11-14T00:00:00\",\"status\":1}," +
                                            "{\"name\":\"Company 3\",\"startDate\":\"2017-11-14T00:00:00\",\"status\":4}]";

            // Act
            var response = await _fixture.WebClient.GetAsync("/jobApplication");

            // Assert
            response.EnsureSuccessStatusCode();
            string responseData = response.Content.ReadAsStringAsync().Result;
            _output.WriteLine(responseData);
            Assert.Equal(expectedResponse, responseData);
        }

        [Fact]
        public async Task ShouldReturnCorrectResponseWhenNoJobApplications()
        {
            // Arrange
            const string expectedResponse = "";

            // Act
            var response = await _fixture.WebClient.GetAsync("/jobApplication");

            // Assert
            response.EnsureSuccessStatusCode();
            string responseData = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(expectedResponse, responseData);
        }
    }
}
