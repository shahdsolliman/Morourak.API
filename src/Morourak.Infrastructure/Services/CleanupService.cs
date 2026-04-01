using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Morourak.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Morourak.Infrastructure.Services
{
    /// <summary>
    /// Background service to clean up expired PendingRegistration and OtpVerification records.
    /// </summary>
    public class CleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public CleanupService(IServiceProvider serviceProvider, ILogger<CleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during cleanup.");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Cleanup Service is stopping.");
        }

        private async Task DoCleanupAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

            var now = DateTime.UtcNow;

            // 1. Clean up PendingRegistration older than 24 hours or expired OTP
            var expiredPending = await dbContext.PendingRegistrations
                .Where(p => p.CreatedAt < now.AddDays(-1) || (p.OtpExpiry.HasValue && p.OtpExpiry < now.AddHours(-1)))
                .ToListAsync();

            if (expiredPending.Any())
            {
                _logger.LogInformation("Cleaning up {Count} expired pending registrations.", expiredPending.Count);
                dbContext.PendingRegistrations.RemoveRange(expiredPending);
            }

            // 2. Clean up generic OtpVerification older than 1 hour
            var expiredOtps = await dbContext.OtpVerifications
                .Where(o => o.Expiry < now || o.CreatedAt < now.AddHours(-1))
                .ToListAsync();

            if (expiredOtps.Any())
            {
                _logger.LogInformation("Cleaning up {Count} expired OTP verifications.", expiredOtps.Count);
                dbContext.OtpVerifications.RemoveRange(expiredOtps);
            }

            if (expiredPending.Any() || expiredOtps.Any())
            {
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
