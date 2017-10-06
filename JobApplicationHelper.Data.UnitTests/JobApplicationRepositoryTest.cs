using JobApplicationHelper.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.Data.Entities;
using JobApplicationHelper.DomainModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JobApplicationHelper.Data.UnitTests
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
                new JobApplication {Name = "Company 1"},
                new JobApplication {Name = "Company 2"},
                new JobApplication {Name = "Company 3"}
            }.AsQueryable();

            // Act
            var result = await _jobApplicationRepository.FindAll();

            // Assert
            Assert.Equal(expectedJobApplications, result);
        }
        private JobApplicationDbContext GetContextWithData()
        {
            var options = new DbContextOptionsBuilder<JobApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new JobApplicationDbContext(options);

            context.JobApplications.Add(new JobApplicationEntity { Id = 1, CompanyName = "Company 1" });
            context.JobApplications.Add(new JobApplicationEntity { Id = 2, CompanyName = "Company 2" });
            context.JobApplications.Add(new JobApplicationEntity { Id = 3, CompanyName = "Company 3" });
            context.SaveChanges();

            return context;
        }

    }
}
