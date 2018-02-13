using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.DomainModels;
using Xunit;
using Xunit.Abstractions;

namespace JAH.Web.IntegrationTests
{
    public class JobApplicationControllerTest : IClassFixture<ClientFixture>, IDisposable
    {
        private const string UriBasePath = "/jobApplications";
        private readonly ITestOutputHelper _output;
        private readonly ClientFixture _fixture;
        private readonly JobApplicationEntity[] _jobApplicationEntities;

        public JobApplicationControllerTest(ITestOutputHelper output, ClientFixture fixture)
        {
            _output = output;
            _fixture = fixture;

            _jobApplicationEntities = new[]
            {
                new JobApplicationEntity
                {
                    Id = 1,
                    CompanyName = "Company 1",
                    ApplicationDate = new DateTime(2017, 11, 13),
                    CurrentStatus = Status.Interview
                },
                new JobApplicationEntity
                {
                    Id = 2,
                    CompanyName = "Company 2",
                    ApplicationDate = new DateTime(2017, 11, 14),
                    CurrentStatus = Status.Applied
                },
                new JobApplicationEntity
                {
                    Id = 3,
                    CompanyName = "Company 3",
                    ApplicationDate = new DateTime(2017, 11, 14),
                    CurrentStatus = Status.Interview
                },
                new JobApplicationEntity
                {
                    Id = 4,
                    CompanyName = "Company 4",
                    ApplicationDate = new DateTime(2017, 10, 9),
                    CurrentStatus = Status.Offer
                },
                new JobApplicationEntity
                {
                    Id = 5,
                    CompanyName = "Company 5",
                    ApplicationDate = new DateTime(2017, 09, 18),
                    CurrentStatus = Status.Rejected
                }
            };
        }

        public void Dispose()
        {
            _fixture.DetachAllEntities();
            _fixture.JobApplicationDbContext.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetAllApplications_MultipleApplications_HtmlView()
        {
            // Arrange
            foreach (JobApplicationEntity jobApplicationEntity in _jobApplicationEntities)
            {
                _fixture.JobApplicationDbContext.JobApplications.Add(jobApplicationEntity);
            }

            _fixture.JobApplicationDbContext.SaveChanges();

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync(UriBasePath);

            // Assert
            response.EnsureSuccessStatusCode();
            string responseData = response.Content.ReadAsStringAsync().Result;
            _output.WriteLine(responseData);
            Assert.NotEmpty(responseData);
        }

        [Fact]
        public async Task GetAllApplications_NoApplications_HtmlView()
        {
            // Arrange

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync(UriBasePath);

            // Assert
            response.EnsureSuccessStatusCode();
            string responseData = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(responseData);
        }

        [Fact]
        public async void CreateNewApplication_ApplicationAlreadyExists_501()
        {
            // Arrange
            _fixture.JobApplicationDbContext.JobApplications.Add(_jobApplicationEntities[0]);
            _fixture.JobApplicationDbContext.SaveChanges();
            _fixture.DetachAllEntities();

            var jobApplication = new JobApplication
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };

            // Act

            var stringContent = new StringContent(jobApplication.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await _fixture.WebClient.PostAsync($"{UriBasePath}/addNewApplication", stringContent);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            string responseData = response.Content.ReadAsStringAsync().Result;
            Assert.Empty(responseData);
        }

        [Fact]
        public async void CreateNewApplication_ApplicationDoesNotExists_OkObjectResult()
        {
            // Arrange
            var jobApplication = new JobApplication
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };

            // Act
            var stringContent = new StringContent(jobApplication.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await _fixture.WebClient.PostAsync($"{UriBasePath}/addNewApplication", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }

        [Fact]
        public async Task GetApplicationByCompanyName_ApplicationExists_HtmlView()
        {
            // Arrange
            foreach (JobApplicationEntity jobApplicationEntity in _jobApplicationEntities)
            {
                _fixture.JobApplicationDbContext.JobApplications.Add(jobApplicationEntity);
            }

            _fixture.JobApplicationDbContext.SaveChanges();

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync($"{UriBasePath}/{_jobApplicationEntities[0].CompanyName}");

            // Assert
            response.EnsureSuccessStatusCode();
            string responseData = response.Content.ReadAsStringAsync().Result;
            _output.WriteLine(responseData);
            Assert.NotEmpty(responseData);
        }

        [Fact]
        public async Task GetApplicationByCompanyName_ApplicationDoesNotExist_HtmlView()
        {
            // Arrange

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync($"{UriBasePath}/{_jobApplicationEntities[0].CompanyName}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void UpdateApplication_ApplicationExists_OkObjectResult()
        {
            // Arrange
            foreach (JobApplicationEntity jobApplicationEntity in _jobApplicationEntities)
            {
                _fixture.JobApplicationDbContext.JobApplications.Add(jobApplicationEntity);
            }

            _fixture.JobApplicationDbContext.SaveChanges();

            var jobApplication = new JobApplication
            {
                CompanyName = _jobApplicationEntities[0].CompanyName,
                ApplicationDate = _jobApplicationEntities[0].ApplicationDate,
                Status = Status.Offer
            };

            // Act
            var stringContent = new StringContent(jobApplication.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await _fixture.WebClient.PostAsync($"{UriBasePath}/updateApplication", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }

        [Fact]
        public async void UpdateApplication_ApplicationDoesNotExist_OkObjectResult()
        {
            // Arrange
            var jobApplication = new JobApplication
            {
                CompanyName = _jobApplicationEntities[0].CompanyName,
                ApplicationDate = _jobApplicationEntities[0].ApplicationDate,
                Status = Status.Offer
            };

            // Act
            var stringContent = new StringContent(jobApplication.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await _fixture.WebClient.PostAsync($"{UriBasePath}/updateApplication", stringContent);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
        }
    }
}
