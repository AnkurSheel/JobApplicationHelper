using System;
using System.Linq;
using System.Threading.Tasks;

using JAH.Data.Entities;
using JAH.DomainModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace JAH.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(JobApplicationDbContext context,
                                            UserManager<JobApplicationUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            ILogger<DbInitializer> logger)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                // database has been seeded
                return;
            }

            var user = await CreateDefaultUserAndRoleForApplication(userManager, roleManager, logger);

            var jobApplication = new JobApplicationEntity {
                                                              CompanyName = "Company 1",
                                                              ApplicationDate = new DateTime(2017, 11, 13),
                                                              CurrentStatus = Status.Applied,
                                                              Owner = user
                                                          };

            context.JobApplications.Add(jobApplication);

            context.SaveChanges();
        }

        private static async Task AddDefaultRoleToDefaultUser(UserManager<JobApplicationUser> userManager,
                                                              ILogger<DbInitializer> logger,
                                                              string administratorRole,
                                                              JobApplicationUser user)
        {
            logger.LogInformation($"Add default user 'admin' to role '{administratorRole}'");
            var ir = await userManager.AddToRoleAsync(user, administratorRole);
            if (ir.Succeeded)
            {
                logger.LogDebug($"Added the role '{administratorRole}' to default user 'admin' successfully");
            }
            else
            {
                var exception = new ApplicationException($"The role `{administratorRole}` cannot be set for the user 'admin'");
                logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private static async Task CreateDefaultAdministratorRole(RoleManager<IdentityRole> roleManager,
                                                                 ILogger<DbInitializer> logger,
                                                                 string administratorRole)
        {
            logger.LogInformation($"Create the role `{administratorRole}` for application");
            var ir = await roleManager.CreateAsync(new IdentityRole(administratorRole));
            if (ir.Succeeded)
            {
                logger.LogDebug($"Created the role `{administratorRole}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default role `{administratorRole}` cannot be created");
                logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private static async Task<JobApplicationUser> CreateDefaultUser(UserManager<JobApplicationUser> userManager, ILogger<DbInitializer> logger)
        {
            logger.LogInformation("Create default user for application");
            const string UserName = "admin";

            var user = new JobApplicationUser(UserName);

            var ir = await userManager.CreateAsync(user);
            if (ir.Succeeded)
            {
                logger.LogDebug("Created default 'user' admin successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default user 'admin' cannot be created");
                logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }

            var createdUser = await userManager.FindByNameAsync(UserName);
            return createdUser;
        }

        private static async Task<JobApplicationUser> CreateDefaultUserAndRoleForApplication(
            UserManager<JobApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DbInitializer> logger)
        {
            const string AdministratorRole = "Administrator";

            await CreateDefaultAdministratorRole(roleManager, logger, AdministratorRole);
            var user = await CreateDefaultUser(userManager, logger);
            await SetPasswordForDefaultUser(userManager, logger, user);
            await AddDefaultRoleToDefaultUser(userManager, logger, AdministratorRole, user);

            return user;
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

        private static async Task SetPasswordForDefaultUser(UserManager<JobApplicationUser> userManager,
                                                            ILogger<DbInitializer> logger,
                                                            JobApplicationUser user)
        {
            logger.LogInformation($"Set password for default user 'admin'");
            const string password = "admin";
            var ir = await userManager.AddPasswordAsync(user, password);
            if (ir.Succeeded)
            {
                logger.LogTrace($"Set password `{password}` for default user 'admin' successfully");
            }
            else
            {
                var exception = new ApplicationException($"Password for the user 'admin' cannot be set");
                logger.LogError(exception, GetIdentityErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }
    }
}