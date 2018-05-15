using Microsoft.EntityFrameworkCore;

namespace JAH.Data
{
    public static class JobApplicationDbContextExtensions
    {
        public static void SeedData(this DbSeeder dbSeeder)
        {
            dbSeeder.SeedDefaultUserAndRoles().Wait();
        }

        public static void UpdateDB(this JobApplicationDbContext context)
        {
            context.Database.Migrate();
        }
    }
}
