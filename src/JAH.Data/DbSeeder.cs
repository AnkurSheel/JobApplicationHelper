using System;
using System.Linq;
using System.Threading.Tasks;

using JAH.Data.Entities;
using JAH.DomainModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace JAH.Data
{
    public class DbSeeder
    {
        private readonly JobApplicationDbContext _context;

        private readonly ILogger<JobApplicationDbContext> _logger;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<JobApplicationUser> _userManager;

        /// <inheritdoc />
        public DbSeeder(JobApplicationDbContext context
                      , UserManager<JobApplicationUser> userManager
                      , RoleManager<IdentityRole> roleManager
                      , ILogger<JobApplicationDbContext> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedDefaultUserAndRoles()
        {
            _context.Database.EnsureCreated();

            if (_context.Users.Any())
            {
                // database has been seeded
                return;
            }

            JobApplicationUser user = await CreateDefaultUserAndRole();

            var jobApplication = new JobApplicationEntity
                                 {
                                     CompanyName = "Company 1"
                                   , ApplicationDate = new DateTime(2017, 11, 13)
                                   , CurrentStatus = Status.Applied
                                   , Owner = user
                                 };

            _context.JobApplications.Add(jobApplication);

            await _context.SaveChangesAsync();
        }

        private async Task AddDefaultRoleToDefaultUser(string administratorRole, JobApplicationUser user)
        {
            _logger.LogInformation($"Add default user 'admin' to role '{administratorRole}'");
            IdentityResult ir = await _userManager.AddToRoleAsync(user, administratorRole);
            if (ir.Succeeded)
            {
                _logger.LogDebug($"Added the role '{administratorRole}' to default user 'admin' successfully");
            }
            else
            {
                var exception = new ApplicationException($"The role `{administratorRole}` cannot be set for the user 'admin'");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private async Task CreateDefaultAdministratorRole(string administratorRole)
        {
            _logger.LogInformation($"Create the role `{administratorRole}` for application");
            IdentityResult ir = await _roleManager.CreateAsync(new IdentityRole(administratorRole));
            if (ir.Succeeded)
            {
                _logger.LogDebug($"Created the role `{administratorRole}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default role `{administratorRole}` cannot be created");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private async Task<JobApplicationUser> CreateDefaultUser()
        {
            _logger.LogInformation("Create default user for application");
            const string UserName = "admin";

            var user = new JobApplicationUser(UserName);

            IdentityResult ir = await _userManager.CreateAsync(user);
            if (ir.Succeeded)
            {
                _logger.LogDebug("Created default 'user' admin successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default user 'admin' cannot be created");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }

            JobApplicationUser createdUser = await _userManager.FindByNameAsync(UserName);
            return createdUser;
        }

        private async Task<JobApplicationUser> CreateDefaultUserAndRole()
        {
            const string AdministratorRole = "Administrator";

            await CreateDefaultAdministratorRole(AdministratorRole);
            JobApplicationUser user = await CreateDefaultUser();
            await SetPasswordForDefaultUser(user);
            await AddDefaultRoleToDefaultUser(AdministratorRole, user);

            return user;
        }

        private string GetIdentityErrorsInCommaSeperatedList(IdentityResult ir)
        {
            string errors = null;
            foreach (IdentityError identityError in ir.Errors)
            {
                errors += identityError.Description;
                errors += ", ";
            }

            return errors;
        }

        private async Task SetPasswordForDefaultUser(JobApplicationUser user)
        {
            _logger.LogInformation($"Set password for default user 'admin'");
            const string password = "admin";
            IdentityResult ir = await _userManager.AddPasswordAsync(user, password);
            if (ir.Succeeded)
            {
                _logger.LogTrace($"Set password `{password}` for default user 'admin' successfully");
            }
            else
            {
                var exception = new ApplicationException($"Password for the user 'admin' cannot be set");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }
    }
}
