using System;
using System.Linq;

using JAH.Data.Entities;
using JAH.Data.Repositories;
using JAH.DomainModels;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace JAH.Data.UnitTests
{
    public class JobApplicationRepositoryTest
    {
        private readonly JobApplicationDbContext _jobApplicationDbContext;

        private readonly JobApplicationEntity[] _jobApplicationEntities;

        private readonly Guid _guid;

        private JobApplicationRepository _jobApplicationRepository;

        public JobApplicationRepositoryTest()
        {
            _guid = Guid.NewGuid();
            _jobApplicationDbContext = ContextFixture.GetContext(_guid);
            _jobApplicationRepository = new JobApplicationRepository(_jobApplicationDbContext);
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
            await _jobApplicationRepository.Create(jobApplication).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, _jobApplicationDbContext.JobApplications.Count());

            JobApplicationEntity entity = _jobApplicationDbContext.JobApplications.FirstOrDefault();

            Assert.Equal(jobApplication, entity);
        }

        [Fact]
        public async void Create_ApplicationExists_ThrowException()
        {
            // Arrange
            var jobApplication = new JobApplicationEntity
            {
                Id = _jobApplicationEntities[0].Id,
                CompanyName = "New company",
                ApplicationDate = new DateTime(2017, 12, 20),
                CurrentStatus = Status.Offer
            };
            _jobApplicationDbContext.JobApplications.Add(_jobApplicationEntities[0]);
            _jobApplicationDbContext.SaveChanges();

            JobApplicationDbContext context = ContextFixture.GetContext(_guid);
            _jobApplicationRepository = new JobApplicationRepository(context);

            // Act
            Exception ex = await Record.ExceptionAsync(async () => await _jobApplicationRepository.Create(jobApplication).ConfigureAwait(false))
                                       .ConfigureAwait(false);

            // Assert
            JobApplicationEntity entity = ContextFixture.GetContext(_guid).JobApplications.FirstOrDefault(x => x.Id == jobApplication.Id);
            Assert.Equal(entity, _jobApplicationEntities[0]);
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
        public void GetOne_ApplicationwithCompanyNameDoesNotExist_ReturnsNull()
        {
            // Arrange

            // Act
            JobApplicationEntity jobApplication = _jobApplicationRepository.GetOne(x => x.CompanyName.Equals(_jobApplicationEntities[0].CompanyName));

            // Assert
            Assert.Null(jobApplication);
        }

        [Fact]
        public async void Update_ApplicationExists_UpdatesJobApplication()
        {
            // Arrange
            var jobApplication = new JobApplicationEntity
            {
                Id = _jobApplicationEntities[0].Id,
                CompanyName = "New company",
                ApplicationDate = new DateTime(2017, 12, 20),
                CurrentStatus = Status.Offer
            };
            _jobApplicationDbContext.JobApplications.Add(_jobApplicationEntities[0]);
            _jobApplicationDbContext.SaveChanges();

            JobApplicationDbContext context = ContextFixture.GetContext(_guid);
            _jobApplicationRepository = new JobApplicationRepository(context);

            // Act
            await _jobApplicationRepository.Update(jobApplication).ConfigureAwait(false);

            // Assert
            JobApplicationEntity entity = ContextFixture.GetContext(_guid).JobApplications.FirstOrDefault(x => x.Id == jobApplication.Id);

            Assert.Equal(jobApplication, entity);
        }

        [Fact]
        public async void Update_ApplicationDoesNotExist_ThrowException()
        {
            var jobApplication = new JobApplicationEntity
            {
                Id = _jobApplicationEntities[0].Id,
                CompanyName = "New company",
                ApplicationDate = new DateTime(2017, 12, 20),
                CurrentStatus = Status.Offer
            };

            // Act
            Exception ex = await Record.ExceptionAsync(async () => await _jobApplicationRepository.Update(jobApplication).ConfigureAwait(false))
                                       .ConfigureAwait(false);

            // Assert
            Assert.IsType<DbUpdateConcurrencyException>(ex);
            Assert.NotNull(ex);
        }
    }
}
