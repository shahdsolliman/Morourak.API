using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    public static class InsuranceCompanySeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            if (await context.InsuranceCompanies.AnyAsync())
            {
                return;
            }

            var companies = new List<InsuranceCompany>
            {
                new InsuranceCompany
                {
                    Name = "Misr Insurance Company",
                    NameAr = "الشركة المصرية للتأمين",
                    Fee = 890m,
                    Description = "Basic coverage: 1 year, premium plan",
                    DescriptionAr = "التغطية الأساسية: سنة، خطة متميزة"
                },
                new InsuranceCompany
                {
                    Name = "Delta Insurance",
                    NameAr = "دلتا للتأمين",
                    Fee = 850m,
                    Description = "Standard coverage",
                    DescriptionAr = "تغطية قياسية"
                },
                new InsuranceCompany
                {
                    Name = "Suez Canal Insurance",
                    NameAr = "تأمين قناة السويس",
                    Fee = 920m,
                    Description = "Comprehensive coverage",
                    DescriptionAr = "تغطية شاملة"
                }
            };

            await context.InsuranceCompanies.AddRangeAsync(companies);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} insurance companies.", companies.Count);
        }
    }
}
