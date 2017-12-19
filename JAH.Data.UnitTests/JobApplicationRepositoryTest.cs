using System;
using System.Linq;
using JAH.Data.Entities;
using JAH.Data.Repositories;
using JAH.DomainModels;
using Xunit;

namespace JAH.Data.UnitTests
{
    public class JobApplicationRepositoryTest
    {
        private readonly JobApplicationRepository _jobApplicationRepository;
        private readonly JobApplicationDbContext _jobApplicationDbContext;
        private readonly JobApplicationEntity[] _jobApplicationEntities;

        public JobApplicationRepositoryTest()
        {
            _jobApplicationDbContext = ContextFixture.GetContextWithData();
            _jobApplicationRepository = new JobApplicationRepository(_jobApplicationDbContext);
            _jobApplicationEntities = new[]
            {
                new JobApplicationEntity
                {
                    CompanyName = "Company 1",
                    ApplicationDate = new DateTime(2017, 11, 13),
                    CurrentStatus = Status.Interview
                },
                new JobApplicationEntity { CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), CurrentStatus = Status.Applied },
                new JobApplicationEntity
                {
                    CompanyName = "Company 3",
                    ApplicationDate = new DateTime(2017, 11, 14),
                    CurrentStatus = Status.Interview
                },
                new JobApplicationEntity { CompanyName = "Company 4", ApplicationDate = new DateTime(2017, 10, 9), CurrentStatus = Status.Offer },
                new JobApplicationEntity { CompanyName = "Company 5", ApplicationDate = new DateTime(2017, 09, 18), CurrentStatus = Status.Rejected }
            };
        }

        [Fact]
        public void GetAll_MultipleApplications_AllJobApplications()
        {
            // Arrange

            foreach (JobApplicationEntity jobApplication in _jobApplicationEntities)
            {
                _jobApplicationDbContext.JobApplications.Add(jobApplication);
            }

            _jobApplicationDbContext.SaveChanges();

            // Act
            IQueryable<JobApplicationEntity> result = _jobApplicationRepository.GetAll();

            // Assert
            Assert.Equal(_jobApplicationEntities, result.ToArray());
        }

        [Fact]
        public void GetAll_NoApplications_EmptyCollection()
        {
            // Arrange

            // Act
            IQueryable<JobApplicationEntity> result = _jobApplicationRepository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async void Create_ApplicationDoesNotExist_InsertJobApplication()
        {
            // Arrange
            var jobApplication = new JobApplicationEntity
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                CurrentStatus = Status.Interview
            };

            // Act
            await _jobApplicationRepository.Create(jobApplication);

            // Assert
            Assert.Equal(1, _jobApplicationDbContext.JobApplications.Count());

            JobApplicationEntity entity = _jobApplicationDbContext.JobApplications.FirstOrDefault();

            Assert.Equal(jobApplication, entity);
        }

        [Fact]
        public async void Create_ApplicationExists_ThrowException()
        {
            var jobApplication = new JobApplicationEntity
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                CurrentStatus = Status.Interview
            };
            _jobApplicationDbContext.JobApplications.Add(jobApplication);
            _jobApplicationDbContext.SaveChanges();

            // Act
            var duplicateJobApplication = new JobApplicationEntity
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                CurrentStatus = Status.Interview
            };
            Exception ex = await Record.ExceptionAsync(async () => await _jobApplicationRepository.Create(duplicateJobApplication));

            // Assert
            Assert.IsType<ArgumentException>(ex);
            Assert.NotNull(ex);
        }

        [Fact]
        public void GetOne_SingleApplicationExists_Application()
        {
            // Arrange

            foreach (JobApplicationEntity jobApplication in _jobApplicationEntities)
            {
                _jobApplicationDbContext.JobApplications.Add(jobApplication);
            }

            _jobApplicationDbContext.SaveChanges();

            // Act
            JobApplicationEntity result = _jobApplicationRepository.GetOne(x => x.CompanyName.Equals(_jobApplicationEntities[0].CompanyName));

            // Assert
            Assert.Equal(_jobApplicationEntities[0], result);
        }

        [Fact]
        public void GetOne_MultipleApplicationsMatched_ThrowsException()
        {
            // Arrange

            foreach (JobApplicationEntity jobApplication in _jobApplicationEntities)
            {
                _jobApplicationDbContext.JobApplications.Add(jobApplication);
            }

            _jobApplicationDbContext.SaveChanges();

            // Act
            Exception ex = Record.Exception(() => _jobApplicationRepository.GetOne(x => x.ApplicationDate == new DateTime(2017, 11, 14)));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.NotNull(ex);
        }

        [Fact]
        public void GetOne_ApplicationwithCompanyNameDoesNotExist_NullResult()
        {
            // Arrange

            // Act
            JobApplicationEntity result = _jobApplicationRepository.GetOne(x => x.CompanyName.Equals(_jobApplicationEntities[0].CompanyName));

            // Assert
            Assert.Null(result);
        }
    }
}
