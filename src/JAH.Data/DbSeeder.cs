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
        public DbSeeder(JobApplicationDbContext context,
                        UserManager<JobApplicationUser> userManager,
                        RoleManager<IdentityRole> roleManager,
                        ILogger<JobApplicationDbContext> logger)
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

            JobApplicationUser user = await CreateDefaultUserAndRole().ConfigureAwait(false);

            var jobApplication = new JobApplicationEntity
                                 {
                                     CompanyName = "Company 1",
                                     ApplicationDate = new DateTime(2017, 11, 13),
                                     CurrentStatus = Status.Applied,
                                     Owner = user
                                 };

            _context.JobApplications.Add(jobApplication);

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static string GetIdentityErrorsInCommaSeperatedList(IdentityResult ir)
        {
            string errors = null;
            foreach (IdentityError identityError in ir.Errors)
            {
                errors += identityError.Description;
                errors += ", ";
            }

            return errors;
        }

        private async Task AddDefaultRoleToDefaultUser(string administratorRole, JobApplicationUser user)
        {
            _logger.LogInformation($"Add default user 'admin' to role '{administratorRole}'");
            IdentityResult ir = await _userManager.AddToRoleAsync(user, administratorRole).ConfigureAwait(false);
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
            IdentityResult ir = await _roleManager.CreateAsync(new IdentityRole(administratorRole)).ConfigureAwait(false);
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

            IdentityResult ir = await _userManager.CreateAsync(user).ConfigureAwait(false);
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

            JobApplicationUser createdUser = await _userManager.FindByNameAsync(UserName).ConfigureAwait(false);
            return createdUser;
        }

        private async Task<JobApplicationUser> CreateDefaultUserAndRole()
        {
            const string AdministratorRole = "Administrator";

            await CreateDefaultAdministratorRole(AdministratorRole).ConfigureAwait(false);
            JobApplicationUser user = await CreateDefaultUser().ConfigureAwait(false);
            await SetPasswordForDefaultUser(user).ConfigureAwait(false);
            await AddDefaultRoleToDefaultUser(AdministratorRole, user).ConfigureAwait(false);

            return user;
        }

        private async Task SetPasswordForDefaultUser(JobApplicationUser user)
        {
            _logger.LogInformation($"Set password for default user 'admin'");
            const string password = "admin";
            IdentityResult ir = await _userManager.AddPasswordAsync(user, password).ConfigureAwait(false);
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
