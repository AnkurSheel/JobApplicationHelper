using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.Data.Repositories;
using JAH.DomainModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JAH.Data.UnitTests
{
    public class JobApplicationRepositoryTest
    {
        private readonly JobApplicationRepository _jobApplicationRepository;

        public JobApplicationRepositoryTest()
        {
            var context = GetContextWithData();
            _jobApplicationRepository = new JobApplicationRepository(context);
        }

        [Fact]
        public async Task ShouldReturnAllJobApplications()
        {
            var expectedJobApplications = new List<JobApplication>
            {
                new JobApplication {Name = "Company 1", StartDate = new DateTime(2017, 11, 13)},
                new JobApplication {Name = "Company 2", StartDate = new DateTime(2017, 11, 14)},
                new JobApplication {Name = "Company 3", StartDate = new DateTime(2017, 11, 14)}
            }.AsQueryable();

            // Act
            var result = await _jobApplicationRepository.FindAll();

            // Assert
            Assert.Equal(expectedJobApplications, result);
        }

        private JobApplicationDbContext GetContextWithData()
        {
            var options = new DbContextOptionsBuilder<JobApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new JobApplicationDbContext(options);

            context.JobApplications.Add(new JobApplicationEntity {Id = 1, CompanyName = "Company 1"});
            context.JobApplications.Add(new JobApplicationEntity {Id = 2, CompanyName = "Company 2"});
            context.JobApplications.Add(new JobApplicationEntity {Id = 3, CompanyName = "Company 3"});
            context.SaveChanges();

            return context;
        }
    }
}
