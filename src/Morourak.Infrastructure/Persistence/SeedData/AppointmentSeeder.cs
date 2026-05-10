using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Morourak.Infrastructure.Persistence.SeedData
{
    public static class AppointmentSeeder
    {
        public static async Task SeedAsync(PersistenceDbContext context, ILogger logger)
        {
            if (await context.ExaminationAppointments.AnyAsync())
            {
                return;
            }

            var citizens = await context.CitizenRegistries.Take(5).ToListAsync();
            var trafficUnits = await context.TrafficUnits.Take(3).ToListAsync();

            if (!citizens.Any() || !trafficUnits.Any())
            {
                logger.LogWarning("Skipping Appointment seeding because no citizens or traffic units exist.");
                return;
            }

            var appointments = new List<Appointment>();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var random = new Random();

            // Create some appointments for today
            foreach (var unit in trafficUnits)
            {
                // Medical Appointments (for Doctors)
                appointments.Add(new Appointment
                {
                    CitizenNationalId = citizens[0].NationalId,
                    Date = today,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(9, 30),
                    Status = AppointmentStatus.Scheduled,
                    Type = AppointmentType.Medical,
                    RequestNumber = "REQ-" + random.Next(10000000, 99999999),
                    GovernorateId = unit.GovernorateId,
                    TrafficUnitId = unit.Id,
                    CreatedAt = DateTime.UtcNow
                });

                // Technical Appointments (for Inspectors)
                appointments.Add(new Appointment
                {
                    CitizenNationalId = citizens[1].NationalId,
                    Date = today,
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(10, 30),
                    Status = AppointmentStatus.Scheduled,
                    Type = AppointmentType.Technical,
                    RequestNumber = "REQ-" + random.Next(10000000, 99999999),
                    GovernorateId = unit.GovernorateId,
                    TrafficUnitId = unit.Id,
                    CreatedAt = DateTime.UtcNow
                });

                // Driving Appointments (for Examinators)
                appointments.Add(new Appointment
                {
                    CitizenNationalId = citizens[2].NationalId,
                    Date = today,
                    StartTime = new TimeOnly(11, 0),
                    EndTime = new TimeOnly(11, 30),
                    Status = AppointmentStatus.Scheduled,
                    Type = AppointmentType.Driving,
                    RequestNumber = "REQ-" + random.Next(10000000, 99999999),
                    GovernorateId = unit.GovernorateId,
                    TrafficUnitId = unit.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Add some completed and pending ones
            appointments.Add(new Appointment
            {
                CitizenNationalId = citizens[3].NationalId,
                Date = today,
                StartTime = new TimeOnly(12, 0),
                EndTime = new TimeOnly(12, 30),
                Status = AppointmentStatus.Completed,
                Type = AppointmentType.Medical,
                RequestNumber = "REQ-" + random.Next(10000000, 99999999),
                GovernorateId = trafficUnits[0].GovernorateId,
                TrafficUnitId = trafficUnits[0].Id,
                CreatedAt = DateTime.UtcNow
            });

            await context.ExaminationAppointments.AddRangeAsync(appointments);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} appointments.", appointments.Count);
        }
    }
}
