using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using AutoMapper;

using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.DomainModels;
using JAH.Services.Interfaces;
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

        private readonly IMapper _mapper;

        private readonly IUserResolverService _userResolver;

        private readonly JobApplicationUser _user;

        public JobApplicationServiceTest()
        {
            _jobApplicationRepository = Substitute.For<IRepository<JobApplicationEntity>>();
            _mapper = Substitute.For<IMapper>();
            _userResolver = Substitute.For<IUserResolverService>();
            _user = new JobApplicationUser("user");

            _jobApplicationService = new JobApplicationService(_jobApplicationRepository, _mapper, _userResolver);

            _jobApplications = new[]
                               {
                                   new JobApplication
                                   {
                                       CompanyName = "Company 1",
                                       ApplicationDate = new DateTime(2017, 11, 13),
                                       Status = Status.Rejected
                                   },
                                   new JobApplication
                                   {
                                       CompanyName = "Company 2",
                                       ApplicationDate = new DateTime(2017, 11, 14),
                                       Status = Status.Applied
                                   },
                                   new JobApplication
                                   {
                                       CompanyName = "Company 3",
                                       ApplicationDate = new DateTime(2017, 11, 14),
                                       Status = Status.Interview
                                   },
                                   new JobApplication
                                   {
                                       CompanyName = "Company 4",
                                       ApplicationDate = new DateTime(2017, 10, 9),
                                       Status = Status.Offer
                                   }
                               };

            _jobApplicationEntities = new List<JobApplicationEntity>();
            foreach (JobApplication jobApplication in _jobApplications)
            {
                var jobApplicationEntity = new JobApplicationEntity
                                           {
                                               Owner = _user,
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

            _userResolver.GetCurrentUser().Returns(_user);
            _jobApplicationRepository.GetAll(Arg.Any<Expression<Func<JobApplicationEntity, bool>>>()).Returns(jobApplicationEntities);
            _mapper.Map<IEnumerable<JobApplication>>(Arg.Any<IEnumerable<JobApplicationEntity>>()).Returns(_jobApplications);

            // Act
            IEnumerable<JobApplication> result = await _jobApplicationService.GetAllApplications().ConfigureAwait(false);

            // Assert
            Assert.Equal(_jobApplications, result);
        }

        [Fact]
        public async Task GetAllApplications_NoApplications_EmptyList()
        {
            // Arrange
            var jobApplicationEntities = new TestAsyncEnumerable<JobApplicationEntity>(new List<JobApplicationEntity>());

            _userResolver.GetCurrentUser().Returns(_user);
            _jobApplicationRepository.GetAll(Arg.Any<Expression<Func<JobApplicationEntity, bool>>>()).Returns(jobApplicationEntities);

            // Act
            IEnumerable<JobApplication> result = await _jobApplicationService.GetAllApplications().ConfigureAwait(false);

            // Assert
            Assert.Equal(new List<JobApplication>(), result);
        }

        [Fact]
        public async Task GetAllApplications_FiltersAgainstUser_CorrectFilterInvoked()
        {
            // Arrange
            var jobApplicationEntities = new TestAsyncEnumerable<JobApplicationEntity>(new List<JobApplicationEntity>());
            Expression<Func<JobApplicationEntity, bool>> actualFilter = null;

            _userResolver.GetCurrentUser().Returns(_user);
            _jobApplicationRepository.GetAll(Arg.Do<Expression<Func<JobApplicationEntity, bool>>>(filter => actualFilter = filter))
                                     .Returns(jobApplicationEntities);

            // Act
            await _jobApplicationService.GetAllApplications().ConfigureAwait(false);

            // Assert
            Func<JobApplicationEntity, bool> compiledActualFilter = actualFilter.Compile();
            Assert.True(compiledActualFilter.Invoke(new JobApplicationEntity { Owner = _user }));
            Assert.False(compiledActualFilter.Invoke(new JobApplicationEntity { Owner = new JobApplicationUser("user1") }));
        }

        [Fact]
        public async void AddNewApplication_ApplicationDoesNotExist_InsertJobApplication()
        {
            // Arrange
            _userResolver.GetCurrentUser().Returns(_user);
            var jobApplicationEntity = new JobApplicationEntity
                                       {
                                           Owner = _user,
                                           CompanyName = _jobApplications[0].CompanyName,
                                           ApplicationDate = _jobApplications[0].ApplicationDate,
                                           CurrentStatus = _jobApplications[0].Status
                                       };

            _mapper.Map(_jobApplications[0], Arg.Any<Action<IMappingOperationOptions<JobApplication, JobApplicationEntity>>>())
                   .Returns(jobApplicationEntity);

            // Act
            await _jobApplicationService.AddNewApplication(_jobApplications[0]).ConfigureAwait(false);

            // Assert
            await _jobApplicationRepository.Received().Create(jobApplicationEntity).ConfigureAwait(false);
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
            _jobApplicationRepository.Create(jobApplicationEntity).Returns(x => throw new ArgumentException("Could not create a new application"));
            _mapper.Map<JobApplication, JobApplicationEntity>(Arg.Any<JobApplication>(), opt => { }).ReturnsForAnyArgs(jobApplicationEntity);

            // Act
            Task<Exception> ex = Record.ExceptionAsync(() => _jobApplicationService.AddNewApplication(_jobApplications[0]));

            // Assert
            await _jobApplicationRepository.Received().Create(jobApplicationEntity).ConfigureAwait(false);
            Assert.NotNull(ex.Result);
        }

        [Fact]
        public void GetApplication_ApplicationExists_JobApplication()
        {
            // Arrange
            const string companyName = "Company 1";
            var jobApplicationEntities = (IEnumerable<JobApplicationEntity>)_jobApplicationEntities;
            JobApplicationEntity jobApplicationEntity = jobApplicationEntities.First(x => x.CompanyName.Equals(companyName));

            _jobApplicationRepository.GetOne(Arg.Any<Expression<Func<JobApplicationEntity, bool>>>()).Returns(jobApplicationEntity);
            _mapper.Map<JobApplication>(jobApplicationEntity).Returns(_jobApplications[0]);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            JobApplication result = _jobApplicationService.GetApplication(companyName);

            // Assert
            Assert.Equal(_jobApplications[0], result);
        }

        [Fact]
        public void GetApplication_NoApplications_Null()
        {
            // Arrange
            const string companyName = "Company 1";

            _jobApplicationRepository.GetOne(Arg.Any<Expression<Func<JobApplicationEntity, bool>>>()).Returns((JobApplicationEntity)null);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            JobApplication result = _jobApplicationService.GetApplication(companyName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetApplication_FiltersAgainstUserAndCompany_FilterInvoked()
        {
            // Arrange
            const string companyName = "Company 1";
            Expression<Func<JobApplicationEntity, bool>> actualFilter = null;

            _jobApplicationRepository.GetOne(Arg.Do<Expression<Func<JobApplicationEntity, bool>>>(filter => actualFilter = filter))
                                     .Returns((JobApplicationEntity)null);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            _jobApplicationService.GetApplication(companyName);

            // Assert
            Func<JobApplicationEntity, bool> compiledActualFilter = actualFilter.Compile();
            Assert.True(compiledActualFilter.Invoke(new JobApplicationEntity { Owner = _user, CompanyName = companyName }));
        }

        [Fact]
        public void GetApplication_FiltersAgainstNotLoggedInUser_FilterNotInvoked()
        {
            // Arrange
            const string companyName = "Company 1";
            Expression<Func<JobApplicationEntity, bool>> actualFilter = null;

            _jobApplicationRepository.GetOne(Arg.Do<Expression<Func<JobApplicationEntity, bool>>>(filter => actualFilter = filter))
                                     .Returns((JobApplicationEntity)null);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            JobApplication result = _jobApplicationService.GetApplication(companyName);

            // Assert
            Func<JobApplicationEntity, bool> compiledActualFilter = actualFilter.Compile();
            Assert.False(compiledActualFilter.Invoke(new JobApplicationEntity
                                                     {
                                                         Owner = new JobApplicationUser("user1"),
                                                         CompanyName = companyName
                                                     }));
            Assert.Null(result);
        }

        [Fact]
        public void GetApplication_FiltersAgainstCompanyDoesNotExistForUser_FilterNotInvoked()
        {
            // Arrange
            const string companyName = "Company 1";
            Expression<Func<JobApplicationEntity, bool>> actualFilter = null;

            _jobApplicationRepository.GetOne(Arg.Do<Expression<Func<JobApplicationEntity, bool>>>(filter => actualFilter = filter))
                                     .Returns((JobApplicationEntity)null);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            _jobApplicationService.GetApplication(companyName);

            // Assert
            Func<JobApplicationEntity, bool> compiledActualFilter = actualFilter.Compile();
            Assert.False(compiledActualFilter.Invoke(new JobApplicationEntity { Owner = _user, CompanyName = "Deleted" }));
        }

        [Fact]
        public async void UpdateApplication_ApplicationExists_CallRepositoryUpdateAndReturnsUpdatedJobApplication()
        {
            // Arrange
            const string companyName = "Company 1";
            var jobApplicationEntities = (IEnumerable<JobApplicationEntity>)_jobApplicationEntities;
            JobApplicationEntity jobApplicationEntity = jobApplicationEntities.First(x => x.CompanyName.Equals(companyName));
            _jobApplicationRepository.GetOne(Arg.Any<Expression<Func<JobApplicationEntity, bool>>>()).Returns(jobApplicationEntity);
            _mapper.Map<JobApplication>(jobApplicationEntity).Returns(_jobApplications[0]);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            JobApplication jobApplication = await _jobApplicationService
                                                  .UpdateApplication(_jobApplications[0].CompanyName, _jobApplications[0])
                                                  .ConfigureAwait(false);

            // Assert
            await _jobApplicationRepository.Received().Update(_jobApplicationEntities[0]).ConfigureAwait(false);

            Assert.Equal(_jobApplications[0], jobApplication);
        }

        [Fact]
        public async void UpdateApplication_ApplicationDoesNotExist_ReturnsNull()
        {
            // Arrange

            // Act
            JobApplication jobApplication = await _jobApplicationService
                                                  .UpdateApplication(_jobApplications[0].CompanyName, _jobApplications[0])
                                                  .ConfigureAwait(false);

            // Assert
            Assert.Null(jobApplication);
        }

        [Fact]
        public async void UpdateApplication_FiltersAgainstUserAndCompany_FilterInvoked()
        {
            // Arrange
            const string companyName = "Company 1";
            Expression<Func<JobApplicationEntity, bool>> actualFilter = null;
            var jobApplicationEntities = (IEnumerable<JobApplicationEntity>)_jobApplicationEntities;

            JobApplicationEntity jobApplicationEntity = jobApplicationEntities.First(x => x.CompanyName.Equals(companyName));
            _jobApplicationRepository.GetOne(Arg.Do<Expression<Func<JobApplicationEntity, bool>>>(filter => actualFilter = filter))
                                     .Returns(jobApplicationEntity);
            _mapper.Map<JobApplication>(jobApplicationEntity).Returns(_jobApplications[0]);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            await _jobApplicationService.UpdateApplication(_jobApplications[0].CompanyName, _jobApplications[0]).ConfigureAwait(false);

            // Assert
            Func<JobApplicationEntity, bool> compiledActualFilter = actualFilter.Compile();
            Assert.True(compiledActualFilter.Invoke(new JobApplicationEntity { Owner = _user, CompanyName = companyName }));
        }

        [Fact]
        public async void UpdateApplication_FiltersAgainstCompanyDoesNotExistForUser_FilterNotInvoked()
        {
            const string companyName = "Company 1";
            Expression<Func<JobApplicationEntity, bool>> actualFilter = null;
            var jobApplicationEntities = (IEnumerable<JobApplicationEntity>)_jobApplicationEntities;

            JobApplicationEntity jobApplicationEntity = jobApplicationEntities.First(x => x.CompanyName.Equals(companyName));
            _jobApplicationRepository.GetOne(Arg.Do<Expression<Func<JobApplicationEntity, bool>>>(filter => actualFilter = filter))
                                     .Returns(jobApplicationEntity);
            _mapper.Map<JobApplication>(jobApplicationEntity).Returns(_jobApplications[0]);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            await _jobApplicationService.UpdateApplication(_jobApplications[0].CompanyName, _jobApplications[0]).ConfigureAwait(false);

            // Assert
            Func<JobApplicationEntity, bool> compiledActualFilter = actualFilter.Compile();
            Assert.False(compiledActualFilter.Invoke(new JobApplicationEntity { Owner = _user, CompanyName = "Deleted" }));
        }

        [Fact]
        public async void UpdateApplication_FiltersAgainstNotLoggedInUser_FilterNotInvoked()
        {
            const string companyName = "Company 1";
            Expression<Func<JobApplicationEntity, bool>> actualFilter = null;
            var jobApplicationEntities = (IEnumerable<JobApplicationEntity>)_jobApplicationEntities;

            JobApplicationEntity jobApplicationEntity = jobApplicationEntities.First(x => x.CompanyName.Equals(companyName));
            _jobApplicationRepository.GetOne(Arg.Do<Expression<Func<JobApplicationEntity, bool>>>(filter => actualFilter = filter))
                                     .Returns(jobApplicationEntity);
            _mapper.Map<JobApplication>(jobApplicationEntity).Returns(_jobApplications[0]);
            _userResolver.GetCurrentUser().Returns(_user);

            // Act
            await _jobApplicationService.UpdateApplication(_jobApplications[0].CompanyName, _jobApplications[0]).ConfigureAwait(false);

            // Assert
            Func<JobApplicationEntity, bool> compiledActualFilter = actualFilter.Compile();
            Assert.False(compiledActualFilter.Invoke(new JobApplicationEntity
                                                     {
                                                         Owner = new JobApplicationUser("user1"),
                                                         CompanyName = companyName
                                                     }));
        }
    }
}
