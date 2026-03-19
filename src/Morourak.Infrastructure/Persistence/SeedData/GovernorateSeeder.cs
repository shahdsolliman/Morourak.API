using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;
using System.Text.Json;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    /// <summary>
    /// يقوم ببذر بيانات المحافظات ووحدات المرور من ملف JSON.
    /// العملية مكررة-آمنة: لا تُضاف محافظة مرتين إذا كانت موجودة بالفعل.
    /// </summary>
    public static class GovernorateSeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "SeedData",
                "governorates.json");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("ملف بذر المحافظات غير موجود في المسار: {Path}", filePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var models = JsonSerializer.Deserialize<List<GovernorateSeedModel>>(jsonData, options);

            if (models == null || !models.Any())
            {
                logger.LogInformation("لا توجد بيانات محافظات في ملف البذر.");
                return;
            }

            // تحميل المحافظات الموجودة مع وحدات مرورها لتجنب التكرار وتحديث البيانات
            var existingGovernorates = await context.Governorates
                .Include(g => g.TrafficUnits)
                .ToListAsync();

            foreach (var model in models)
            {
                var governorate = existingGovernorates.FirstOrDefault(g => g.Name == model.Name);

                if (governorate == null)
                {
                    // محافظة جديدة
                    governorate = new Governorate
                    {
                        Name = model.Name,
                        TrafficUnits = model.TrafficUnits
                            .Select(u => new TrafficUnit 
                            { 
                                Name = u.Name,
                                Address = u.Address,
                                WorkingHours = u.WorkingHours
                            })
                            .ToList()
                    };
                    await context.Governorates.AddAsync(governorate);
                    logger.LogInformation("إضافة محافظة جديدة: {Name}", model.Name);
                }
                else
                {
                    // محافظة موجودة مسبقاً — نتحقق من وحدات المرور التابعة لها
                    foreach (var unitModel in model.TrafficUnits)
                    {
                        var trafficUnit = governorate.TrafficUnits.FirstOrDefault(u => u.Name == unitModel.Name);

                        if (trafficUnit == null)
                        {
                            // وحدة مرور جديدة للمحافظة
                            governorate.TrafficUnits.Add(new TrafficUnit
                            {
                                Name = unitModel.Name,
                                Address = unitModel.Address,
                                WorkingHours = unitModel.WorkingHours
                            });
                            logger.LogInformation("إضافة وحدة مرور جديدة [{Unit}] لمحافظة [{Gov}]", unitModel.Name, model.Name);
                        }
                        else
                        {
                            // وحدة مرور موجودة — نحدّث البيانات الإضافية فقط (العنوان وساعات العمل)
                            bool updated = false;
                            
                            if (trafficUnit.Address != unitModel.Address)
                            {
                                trafficUnit.Address = unitModel.Address;
                                updated = true;
                            }
                            
                            if (trafficUnit.WorkingHours != unitModel.WorkingHours)
                            {
                                trafficUnit.WorkingHours = unitModel.WorkingHours;
                                updated = true;
                            }

                            if (updated)
                            {
                                logger.LogInformation("تحديث بيانات وحدة المرور: {Unit}", unitModel.Name);
                            }
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("اكتملت عملية بذر/تحديث بيانات المحافظات ووحدات المرور.");
        }
    }
}
