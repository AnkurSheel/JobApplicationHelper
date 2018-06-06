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
        private const int NumberOfCompaniesToAdd = 2;

        private const int NumberOfTestUsersToAdd = 2;

        private const string AdministratorRole = "Administrator";

        private const string FreeUserRole = "FreeUser";

        private readonly JobApplicationDbContext _context;

        private readonly ILogger<JobApplicationDbContext> _logger;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<JobApplicationUser> _userManager;

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

            await CreateUserAndRole(AdministratorRole, "admin", "admin").ConfigureAwait(false);

            for (var i = 0; i < NumberOfTestUsersToAdd; i++)
            {
                var user = await CreateUserAndRole(FreeUserRole, $"test{i}", $"test{i}").ConfigureAwait(false);
                for (var j = 0; j < NumberOfCompaniesToAdd; j++)
                {
                    var jobApplication = new JobApplicationEntity
                    {
                        CompanyName = $"{user.UserName} Company {j}",
                        ApplicationDate = new DateTime(2017, 11, 13),
                        CurrentStatus = Status.Applied,
                        Owner = user
                    };
                    _context.JobApplications.Add(jobApplication);
                }
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        private static string GetIdentityErrorsInCommaSeperatedList(IdentityResult ir)
        {
            string errors = null;
            foreach (var identityError in ir.Errors)
            {
                errors += identityError.Description;
                errors += ", ";
            }

            return errors;
        }

        private async Task<JobApplicationUser> CreateUserAndRole(string roleName, string userName, string password)
        {
            await CreateRole(roleName).ConfigureAwait(false);
            var user = await CreateUser(userName).ConfigureAwait(false);
            await SetPasswordForUser(user, password).ConfigureAwait(false);
            await AddRoleToUser(roleName, user).ConfigureAwait(false);

            return user;
        }

        private async Task CreateRole(string roleName)
        {
            if (_roleManager.Roles.Any(r => r.Name.Equals(roleName, StringComparison.Ordinal)))
            {
                _logger.LogInformation($"Role `{roleName}` already created");
                return;
            }

            _logger.LogInformation($"Create the role `{roleName}` for application");
            var ir = await _roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
            if (ir.Succeeded)
            {
                _logger.LogDebug($"Created the role `{roleName}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default role `{roleName}` cannot be created");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private async Task AddRoleToUser(string role, JobApplicationUser user)
        {
            _logger.LogInformation($"Add user {user.UserName}' to role '{role}'");
            var ir = await _userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
            if (ir.Succeeded)
            {
                _logger.LogDebug($"Added the role '{role}' to default user 'admin' successfully");
            }
            else
            {
                var exception = new ApplicationException($"The role `{role}` cannot be set for the user 'admin'");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private async Task<JobApplicationUser> CreateUser(string userName)
        {
            _logger.LogInformation($"Create user {userName} for application");

            var user = new JobApplicationUser(userName);

            var ir = await _userManager.CreateAsync(user).ConfigureAwait(false);
            if (ir.Succeeded)
            {
                _logger.LogDebug("Created '{userName}' admin successfully");
            }
            else
            {
                var exception = new ApplicationException($"user '{userName}' cannot be created");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }

            var createdUser = await _userManager.FindByNameAsync(userName).ConfigureAwait(false);
            return createdUser;
        }

        private async Task SetPasswordForUser(JobApplicationUser user, string password)
        {
            _logger.LogInformation($"Set password for default user '{user.UserName}'");
            var ir = await _userManager.AddPasswordAsync(user, password).ConfigureAwait(false);
            if (ir.Succeeded)
            {
                _logger.LogTrace($"Set password `{password}` for default user '{user.UserName}' successfully");
            }
            else
            {
                var exception = new ApplicationException($"Password for the user '{user.UserName}' cannot be set");
                _logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }
    }
}
