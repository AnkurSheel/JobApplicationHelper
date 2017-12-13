using System;
using System.Collections.Generic;
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

        public JobApplicationServiceTest()
        {
            _jobApplicationRepository = Substitute.For<IRepository<JobApplicationEntity>>();
            _jobApplicationService = new JobApplicationService(_jobApplicationRepository);
            _jobApplications = new[]
            {
                new JobApplication {Name = "Company 1", StartDate = new DateTime(2017, 11, 13), Status = Status.None},
                new JobApplication {Name = "Company 2", StartDate = new DateTime(2017, 11, 14), Status = Status.Applied},
                new JobApplication {Name = "Company 3", StartDate = new DateTime(2017, 11, 14), Status = Status.Interview},
                new JobApplication {Name = "Company 4", StartDate = new DateTime(2017, 10, 9), Status = Status.Offer},
                new JobApplication {Name = "Company 5", StartDate = new DateTime(2017, 09, 18), Status = Status.Rejected}
            };
        }

        [Fact]
        public async void ShouldInsertJobApplicationsIfItDoesntAlreadyExist()
        {
            var jobApplicationEntity = new JobApplicationEntity
            {
                CompanyName = _jobApplications[0].Name,
                ApplicationDate = _jobApplications[0].StartDate,
                CurrentStatus = _jobApplications[0].Status
            };

            // Act
            await _jobApplicationService.Add(_jobApplications[0]);

            // Assert
            await _jobApplicationRepository.Received().Add(jobApplicationEntity);
        }

        [Fact]
        public async Task ShouldReturnAllJobApplications()
        {
            var jobApplicationEntities = new TestAsyncEnumerable<JobApplicationEntity>(new List<JobApplicationEntity>
            {
                new JobApplicationEntity {CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), CurrentStatus = Status.None},
                new JobApplicationEntity {CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), CurrentStatus = Status.Applied},
                new JobApplicationEntity {CompanyName = "Company 3", ApplicationDate = new DateTime(2017, 11, 14), CurrentStatus = Status.Interview},
                new JobApplicationEntity {CompanyName = "Company 4", ApplicationDate = new DateTime(2017, 10, 9), CurrentStatus = Status.Offer},
                new JobApplicationEntity {CompanyName = "Company 5", ApplicationDate = new DateTime(2017, 09, 18), CurrentStatus = Status.Rejected}
            });

            _jobApplicationRepository.FindAll().Returns(jobApplicationEntities);

            // Act
            var result = await _jobApplicationService.ReadAllAsync();

            // Assert
            Assert.Equal(_jobApplications, result);
        }

        [Fact]
        public async Task ShouldThrowExceptionIfAddingApplicationAndItAlreadyExist()
        {
            var jobApplicationEntity = new JobApplicationEntity
            {
                CompanyName = _jobApplications[0].Name,
                ApplicationDate = _jobApplications[0].StartDate,
                CurrentStatus = _jobApplications[0].Status
            };
            _jobApplicationRepository.Add(jobApplicationEntity).Returns(x => throw new ArgumentException());

            // Act
            var ex = Record.ExceptionAsync(() => _jobApplicationService.Add(_jobApplications[0]));

            // Assert
            await _jobApplicationRepository.Received().Add(jobApplicationEntity);
            Assert.NotNull(ex.Result);
        }
    }
}
