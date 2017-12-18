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

        public JobApplicationRepositoryTest()
        {
            _jobApplicationDbContext = ContextFixture.GetContextWithData();
            _jobApplicationRepository = new JobApplicationRepository(_jobApplicationDbContext);
        }

        [Fact]
        public void FindAll_MultipleApplications_AllJobApplications()
        {
            // Arrange
            var jobApplications = new[]
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

            foreach (JobApplicationEntity jobApplication in jobApplications)
            {
                _jobApplicationDbContext.JobApplications.Add(jobApplication);
            }

            _jobApplicationDbContext.SaveChanges();

            // Act
            IQueryable<JobApplicationEntity> result = _jobApplicationRepository.FindAll();

            // Assert
            Assert.Equal(jobApplications, result.ToArray());
        }

        [Fact]
        public void FindAll_NoApplications_EmptyCollection()
        {
            // Arrange

            // Act
            IQueryable<JobApplicationEntity> result = _jobApplicationRepository.FindAll();

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
            Assert.NotNull(ex);
        }
    }
}
