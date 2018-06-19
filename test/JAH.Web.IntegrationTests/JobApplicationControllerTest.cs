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
        private readonly ClientFixture _fixture;

        private readonly JobApplicationEntity[] _jobApplicationEntities;

        private readonly Uri _baseUri;

        public JobApplicationControllerTest(ClientFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _fixture.SetupLogger(output);
            _baseUri = new Uri(_fixture.WebClient.BaseAddress, "jobApplications/");
            _fixture.SetupJwtAuthentication();

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

        [Fact]
        public async void CreateNewApplication_ApplicationAlreadyExists_BadRequest()
        {
            // Arrange
            _fixture.Context.JobApplications.Add(_jobApplicationEntities[0]);
            _fixture.Context.SaveChanges();
            _fixture.DetachAllEntities();

            var jobApplication = new JobApplication
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };

            // Act
            var stringContent = new StringContent(jobApplication.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "addNewApplication"), stringContent).ConfigureAwait(false);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
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
            var response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "addNewApplication"), stringContent).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetAllApplications_MultipleApplications_HtmlView()
        {
            // Arrange
            foreach (var jobApplicationEntity in _jobApplicationEntities)
            {
                _fixture.Context.JobApplications.Add(jobApplicationEntity);
            }

            _fixture.Context.SaveChanges();

            // Act
            var response = await _fixture.WebClient.GetAsync(_baseUri).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(responseData);
        }

        [Fact]
        public async Task GetAllApplications_NoApplications_HtmlView()
        {
            // Arrange

            // Act
            var response = await _fixture.WebClient.GetAsync(_baseUri).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(responseData);
        }

        [Fact]
        public async Task GetApplicationByCompanyName_ApplicationDoesNotExist_HtmlView()
        {
            // Arrange

            // Act
            var response = await _fixture.WebClient.GetAsync(new Uri(_baseUri, $"{_jobApplicationEntities[0].CompanyName}")).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task GetApplicationByCompanyName_ApplicationExists_HtmlView()
        {
            // Arrange
            foreach (var jobApplicationEntity in _jobApplicationEntities)
            {
                _fixture.Context.JobApplications.Add(jobApplicationEntity);
            }

            _fixture.Context.SaveChanges();

            // Act
            var response = await _fixture.WebClient.GetAsync(new Uri(_baseUri, $"{_jobApplicationEntities[0].CompanyName}")).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(responseData);
        }

        [Fact]
        public async void UpdateApplication_ApplicationDoesNotExist_NotFoundResult()
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
            var response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "updateApplication"), stringContent).ConfigureAwait(false);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async void UpdateApplication_ApplicationExists_OkObjectResult()
        {
            // Arrange
            foreach (var jobApplicationEntity in _jobApplicationEntities)
            {
                _fixture.Context.JobApplications.Add(jobApplicationEntity);
            }

            _fixture.Context.SaveChanges();
            _fixture.DetachAllEntities();

            var jobApplication = new JobApplication
            {
                CompanyName = _jobApplicationEntities[0].CompanyName,
                ApplicationDate = _jobApplicationEntities[0].ApplicationDate,
                Status = Status.Offer
            };

            // Act
            var stringContent = new StringContent(jobApplication.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "updateApplication"), stringContent).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fixture.EmptyDatabase();
            }
        }
    }
}
