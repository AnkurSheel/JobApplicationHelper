using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IList<JobApplicationEntity> _jobApplicationEntities;
        private IMapper _mapper;

        public JobApplicationServiceTest()
        {
            _jobApplicationRepository = Substitute.For<IRepository<JobApplicationEntity>>();
            _mapper = Substitute.For<IMapper>();
            _jobApplicationService = new JobApplicationService(_jobApplicationRepository, _mapper);

            _jobApplications = new[]
            {
                new JobApplication { Id = 1, CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), Status = Status.Rejected },
                new JobApplication { Id = 2, CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Applied },
                new JobApplication { Id = 3, CompanyName = "Company 3", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Interview },
                new JobApplication { Id = 4, CompanyName = "Company 4", ApplicationDate = new DateTime(2017, 10, 9), Status = Status.Offer }
            };

            _jobApplicationEntities = new List<JobApplicationEntity>();
            foreach (JobApplication jobApplication in _jobApplications)
            {
                var jobApplicationEntity = new JobApplicationEntity
                {
                    Id = jobApplication.Id,
                    CompanyName = jobApplication.CompanyName,
                    ApplicationDate = jobApplication.ApplicationDate,
                    CurrentStatus = jobApplication.Status
                };
                _jobApplicationEntities.Add(jobApplicationEntity);
            }
        }

        [Fact]
        public async Task GetAllApplications_MultipleApplications_AllJobApplications()
        {
            // Arrange
            var jobApplicationEntities = new TestAsyncEnumerable<JobApplicationEntity>(_jobApplicationEntities);

            _jobApplicationRepository.GetAll().Returns(jobApplicationEntities);
            _mapper.Map<IEnumerable<JobApplication>>(Arg.Any<IEnumerable<JobApplicationEntity>>()).Returns(_jobApplications);

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
                Id = _jobApplications[0].Id,
                CompanyName = _jobApplications[0].CompanyName,
                ApplicationDate = _jobApplications[0].ApplicationDate,
                CurrentStatus = _jobApplications[0].Status
            };
            _mapper.Map<JobApplicationEntity>(_jobApplications[0]).Returns(jobApplicationEntity); 

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
                Id = _jobApplications[0].Id,
                CompanyName = _jobApplications[0].CompanyName,
                ApplicationDate = _jobApplications[0].ApplicationDate,
                CurrentStatus = _jobApplications[0].Status
            };
            _jobApplicationRepository.Create(jobApplicationEntity).Returns(x => throw new ArgumentException());
            _mapper.Map<JobApplicationEntity>(Arg.Any<JobApplication>()).Returns(jobApplicationEntity);

            // Act
            Task<Exception> ex = Record.ExceptionAsync(() => _jobApplicationService.AddNewApplication(_jobApplications[0]));

            // Assert
            await _jobApplicationRepository.Received().Create(jobApplicationEntity);
            Assert.NotNull(ex.Result);
        }

        [Fact]
        public void GetApplication_ApplicationExists_JobApplication()
        {
            // Arrange
            const string companyName = "Company 1";
            var jobApplicationEntities = (IEnumerable<JobApplicationEntity>)_jobApplicationEntities;
            JobApplicationEntity jobApplicationEntity = jobApplicationEntities.First(x => x.CompanyName.Equals(companyName));
            _jobApplicationRepository.GetOne().ReturnsForAnyArgs(jobApplicationEntity);
            _mapper.Map<JobApplication>(jobApplicationEntity).Returns(_jobApplications[0]);

            // Act
            JobApplication result = _jobApplicationService.GetApplication(companyName);

            // Assert
            Assert.Equal(_jobApplications[0], result);
        }

        [Fact]
        public void GetApplication_NoApplications_Null()
        {
            // Arrange
            const string company = "Company 1";
            _jobApplicationRepository.GetOne().ReturnsForAnyArgs((JobApplicationEntity)null);

            // Act
            JobApplication result = _jobApplicationService.GetApplication(company);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void UpdateApplication_CallRepositoryUpdate()
        {
            // Arrange

            // Act
            await _jobApplicationService.UpdateApplication(_jobApplications[0], _jobApplications[0]);

            // Assert
            await _jobApplicationRepository.Received().Update(_jobApplicationEntities[0]);
        }
    }
}
