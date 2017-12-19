using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.DomainModels;
using JAH.Services.Services;
using NSubstitute;
using Xunit;

namespace JAH.Services.UnitTests
{
    public class JobApplicationServiceTest
    {
        private readonly IRepository<JobApplicationEntity> _jobApplicationRepository;
        private readonly JobApplication[] _jobApplications;
        private readonly JobApplicationService _jobApplicationService;
        private readonly TestAsyncEnumerable<JobApplicationEntity> _jobApplicationEntities;

        public JobApplicationServiceTest()
        {
            _jobApplicationRepository = Substitute.For<IRepository<JobApplicationEntity>>();
            _jobApplicationService = new JobApplicationService(_jobApplicationRepository);
            _jobApplications = new[]
            {
                new JobApplication { CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), Status = Status.Rejected },
                new JobApplication { CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Applied },
                new JobApplication { CompanyName = "Company 3", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Interview },
                new JobApplication { CompanyName = "Company 4", ApplicationDate = new DateTime(2017, 10, 9), Status = Status.Offer }
            };

            _jobApplicationEntities = new TestAsyncEnumerable<JobApplicationEntity>(new List<JobApplicationEntity>
            {
                new JobApplicationEntity { CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), CurrentStatus = Status.Rejected },
                new JobApplicationEntity { CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), CurrentStatus = Status.Applied },
                new JobApplicationEntity
                {
                    CompanyName = "Company 3",
                    ApplicationDate = new DateTime(2017, 11, 14),
                    CurrentStatus = Status.Interview
                },
                new JobApplicationEntity { CompanyName = "Company 4", ApplicationDate = new DateTime(2017, 10, 9), CurrentStatus = Status.Offer }
            });
        }

        [Fact]
        public async Task GetAllApplications_MultipleApplications_AllJobApplications()
        {
            // Arrange
            _jobApplicationRepository.GetAll().Returns(_jobApplicationEntities);

            // Act
            IEnumerable<JobApplication> result = await _jobApplicationService.GetAllApplications();

            // Assert
            Assert.Equal(_jobApplications, result);
        }

        [Fact]
        public async Task GetAllApplications_NoApplications_AllJobApplications()
        {
            // Arrange
            var jobApplicationEntities = new TestAsyncEnumerable<JobApplicationEntity>(new List<JobApplicationEntity>());
            _jobApplicationRepository.GetAll().Returns(jobApplicationEntities);

            // Act
            IEnumerable<JobApplication> result = await _jobApplicationService.GetAllApplications();

            // Assert
            Assert.Equal(new List<JobApplication>(), result);
        }

        [Fact]
        public async void AddNewApplication_ApplicationDoesNotExist_InsertJobApplication()
        {
            // Arrange
            var jobApplicationEntity = new JobApplicationEntity
            {
                CompanyName = _jobApplications[0].CompanyName,
                ApplicationDate = _jobApplications[0].ApplicationDate,
                CurrentStatus = _jobApplications[0].Status
            };

            // Act
            await _jobApplicationService.AddNewApplication(_jobApplications[0]);

            // Assert
            await _jobApplicationRepository.Received().Create(jobApplicationEntity);
        }

        [Fact]
        public async Task AddNewApplication_ApplicationExists_ThrowException()
        {
            // Arrange
            var jobApplicationEntity = new JobApplicationEntity
            {
                CompanyName = _jobApplications[0].CompanyName,
                ApplicationDate = _jobApplications[0].ApplicationDate,
                CurrentStatus = _jobApplications[0].Status
            };
            _jobApplicationRepository.Create(jobApplicationEntity).Returns(x => throw new ArgumentException());

            // Act
            Task<Exception> ex = Record.ExceptionAsync(() => _jobApplicationService.AddNewApplication(_jobApplications[0]));

            // Assert
            await _jobApplicationRepository.Received().Create(jobApplicationEntity);
            Assert.NotNull(ex.Result);
        }

        [Fact]
        public async Task GetApplication_ApplicationExists_JobApplication()
        {
            // Arrange
            const string companyName = "Company 1";
            var jobApplicationEntities = (IEnumerable<JobApplicationEntity>)_jobApplicationEntities;
            _jobApplicationRepository.GetOne().ReturnsForAnyArgs(jobApplicationEntities.First(x => x.CompanyName.Equals(companyName)));

            // Act
            JobApplication result = await _jobApplicationService.GetApplication(companyName);

            // Assert
            Assert.Equal(_jobApplications[0], result);
        }

        [Fact]
        public async Task GetApplication_NoApplications_Null()
        {
            // Arrange
            const string company = "Company 1";
            _jobApplicationRepository.GetOne().ReturnsForAnyArgs((JobApplicationEntity)null);

            // Act
            JobApplication result = await _jobApplicationService.GetApplication(company);

            // Assert
            Assert.Null(result);
        }
    }
}
