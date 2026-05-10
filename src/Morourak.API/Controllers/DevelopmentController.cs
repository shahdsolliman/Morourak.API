using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Morourak.Domain.Entities;
using Morourak.Infrastructure.Persistence;

namespace Morourak.API.Controllers;

[Route("api/v1/[controller]")]
public class DevelopmentController : BaseApiController
{
    private readonly PersistenceDbContext _context;

    public DevelopmentController(PersistenceDbContext context)
    {
        _context = context;
    }

    [HttpDelete("cleanup-test-requests")]
    public async Task<IActionResult> CleanupRequests()
    {
        var requestNumbers = new[] { "RPL-2026-302", "RPL-2026-301" };

        var requests = await _context.ServiceRequests
            .Include(r => r.Payments)
            .Where(r => requestNumbers.Contains(r.RequestNumber))
            .ToListAsync();

        if (!requests.Any())
            return NotFound("لم يتم العثور على الطلبات المذكورة.");

        foreach (var request in requests)
        {
            await DeleteRequestDataInternal(request);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "تم حذف الطلبات والبيانات المتعلقة بها بنجاح.", deletedRequests = requestNumbers });
    }

    [HttpDelete("cleanup-citizen/{nationalId}")]
    public async Task<IActionResult> CleanupCitizenRequests(string nationalId)
    {
        var citizen = await _context.CitizenRegistries
            .FirstOrDefaultAsync(c => c.NationalId == nationalId);

        if (citizen == null)
            return NotFound($"المواطن ذو الرقم القومي {nationalId} غير موجود في سجلات الأحوال المدنية.");

        // 1. Delete all Service Requests and related data (Payments, Appointments, Applications)
        var requests = await _context.ServiceRequests
            .Include(r => r.Payments)
            .Where(r => r.CitizenNationalId == nationalId)
            .ToListAsync();

        foreach (var request in requests)
        {
            await DeleteRequestDataInternal(request);
        }

        // 2. Delete orphaned applications (those that don't have a ServiceRequest yet)
        var dlApps = await _context.DrivingLicenseApplications
            .Where(a => a.CitizenRegistryId == citizen.Id)
            .ToListAsync();
        _context.DrivingLicenseApplications.RemoveRange(dlApps);

        var renApps = await _context.RenewalApplications
            .Where(a => a.CitizenRegistryId == citizen.Id)
            .ToListAsync();
        _context.RenewalApplications.RemoveRange(renApps);

        var vApps = await _context.VehicleLicenseApplications
            .Where(a => a.CitizenRegistryId == citizen.Id)
            .ToListAsync();
        _context.VehicleLicenseApplications.RemoveRange(vApps);

        await _context.SaveChangesAsync();

        return Ok(new { 
            message = $"تم حذف جميع الطلبات والبيانات المرتبطة بها للمواطن ({nationalId}) بنجاح. تم الإبقاء على الرخص والمخالفات الحالية.", 
            citizenName = $"{citizen.FirstName} {citizen.LastName}",
            deletedRequests = requests.Count
        });
    }

    private async Task DeleteRequestDataInternal(ServiceRequest request)
    {
        // 1. Delete Appointments
        var appointments = await _context.Set<Morourak.Domain.Entities.Appointment>()
            .Where(a => a.RequestNumber == request.RequestNumber)
            .ToListAsync();
        _context.RemoveRange(appointments);

        // 2. Delete Payments
        _context.Payments.RemoveRange(request.Payments);

        // 3. Delete Application based on type
        switch (request.ServiceType)
        {
            case Morourak.Domain.Enums.Request.ServiceType.DrivingLicenseIssue:
                var dlApp = await _context.Set<DrivingLicenseApplication>().FindAsync(request.ReferenceId);
                if (dlApp != null) _context.Remove(dlApp);
                break;
            case Morourak.Domain.Enums.Request.ServiceType.DrivingLicenseRenewal:
                var renewApp = await _context.Set<RenewalApplication>().FindAsync(request.ReferenceId);
                if (renewApp != null) _context.Remove(renewApp);
                break;
            case Morourak.Domain.Enums.Request.ServiceType.VehicleLicenseIssue:
            case Morourak.Domain.Enums.Request.ServiceType.VehicleLicenseRenewal:
                var vApp = await _context.Set<VehicleLicenseApplication>().FindAsync(request.ReferenceId);
                if (vApp != null) _context.Remove(vApp);
                break;
        }

        // 4. Delete ServiceRequest
        _context.ServiceRequests.Remove(request);
    }
}
