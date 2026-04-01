using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Morourak.Application.Common;
using Morourak.Application.DTOs.Admin;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Constants;
using Morourak.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Morourak.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork unitOfWork,
        ILogger<AdminUserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResponse<List<UserDto>>> GetUsersAsync(UserFilterDto filter)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(u => 
                u.Email.Contains(filter.Search) || 
                u.FirstName.Contains(filter.Search) || 
                u.LastName.Contains(filter.Search));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        query = filter.SortBy switch
        {
            "Email" => filter.IsDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "Name" => filter.IsDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            _ => filter.IsDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        var totalRecords = await query.CountAsync();
        var users = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email!,
                Role = userRoles.FirstOrDefault() ?? "None",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return new PagedResponse<List<UserDto>>(userDtos, filter.PageNumber, filter.PageSize, totalRecords);
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return ApiResponse<UserDto>.FailureResult("Email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = dto.IsActive,
            IsVerified = true,
            NationalId = "00000000000000"
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ApiResponse<UserDto>.FailureResult("Failed to create user.", result.Errors.Select(e => e.Description).ToList());

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return ApiResponse<UserDto>.FailureResult($"Role '{dto.Role}' does not exist.");

        await _userManager.AddToRoleAsync(user, dto.Role);

        return ApiResponse<UserDto>.SuccessResult(new UserDto
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = dto.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "User created successfully.");
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return ApiResponse<UserDto>.FailureResult("User not found.");

        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;

        if (dto.Email != null && dto.Email != user.Email)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return ApiResponse<UserDto>.FailureResult("Email already exists.");
            
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return ApiResponse<UserDto>.FailureResult("Failed to update user.");

        if (dto.Role != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        return ApiResponse<UserDto>.SuccessResult(new UserDto
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email!,
            Role = userRoles.FirstOrDefault() ?? "None",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "User updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return ApiResponse<bool>.FailureResult("User not found.");

        // Protection: Prevent deletion of primary Admin
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(AppIdentityConstants.Roles.Admin) && user.Email == "admin@morourak.com")
            return ApiResponse<bool>.FailureResult("Primary administrator cannot be deleted.");

        List<string> filesToDelete = new();

        try
        {
            _logger.LogInformation("Starting deletion of user {Email} (NationalId: {NationalId})", user.Email, user.NationalId);

            await _unitOfWork.BeginTransactionAsync();

            // 1) Cleanup business data in PersistenceDbContext (transactional).
            filesToDelete = await CleanupUserDataAsync(user.NationalId, user.Email!, user.PhoneNumber);

            // Flush deletes inside the transaction so we catch FK/constraint issues before Identity deletion.
            await _unitOfWork.CommitAsync();

            // 2) Hard delete user from Identity database (separate context).
            // If this fails, we roll back the business-data transaction to avoid partial deletion.
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    "Failed to delete user {Email} from Identity DB: {Errors}",
                    user.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return ApiResponse<bool>.FailureResult(
                    "Failed to delete user from Identity database.",
                    result.Errors.Select(e => e.Description).ToList());
            }

            await _unitOfWork.CommitTransactionAsync();

            // 3) Best-effort deletion of uploaded files after successful DB commits.
            DeleteFilesBestEffort(filesToDelete);

            _logger.LogInformation("User {Email} and related data deleted successfully.", user.Email);
            return ApiResponse<bool>.SuccessResult(true, "User and related data fully deleted successfully.");
        }
        catch (Exception ex)
        {
            try { await _unitOfWork.RollbackTransactionAsync(); } catch { /* best-effort */ }
            _logger.LogError(ex, "Error occurred while deleting user {Email}", user.Email);
            return ApiResponse<bool>.FailureResult($"An error occurred during deletion: {ex.Message}");
        }
    }

    private async Task<List<string>> CleanupUserDataAsync(string nationalId, string email, string? phoneNumber)
    {
        _logger.LogInformation("Cleaning up business data for NationalId: {NationalId}, Email: {Email}", nationalId, email);

        var filesToDelete = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Find Citizen ID to handle entities linked by ID rather than NationalId
        var citizen = await _unitOfWork.Repository<CitizenRegistry>()
            .GetAsync(c => c.NationalId == nationalId);

        // 1. Delete Payments (linked by NationalId) - cascades PaymentItems/PaymentViolations
        var payments = await _unitOfWork.Repository<Payment>().FindAsync(p => p.CitizenNationalId == nationalId);
        foreach (var p in payments)
            _unitOfWork.Repository<Payment>().Remove(p);

        // 2. Delete Service Requests (linked by NationalId)
        var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAsync(s => s.CitizenNationalId == nationalId);
        foreach (var sr in serviceRequests)
            _unitOfWork.Repository<ServiceRequest>().Remove(sr);

        // 3. Delete Appointments (linked by NationalId)
        var appointments = await _unitOfWork.Repository<Appointment>().FindAsync(a => a.CitizenNationalId == nationalId);
        foreach (var a in appointments)
            _unitOfWork.Repository<Appointment>().Remove(a);

        // 4. Delete Email OTPs (linked by Email)
        var otps = await _unitOfWork.Repository<EmailOtp>().FindAsync(o => o.Email == email);
        foreach (var o in otps)
            _unitOfWork.Repository<EmailOtp>().Remove(o);

        // 5. Delete Pending Registrations (linked by Email/Phone/NationalId)
        var pendingRegs = await _unitOfWork.Repository<PendingRegistration>()
            .FindAsync(p =>
                p.NationalId == nationalId ||
                p.Email == email ||
                (!string.IsNullOrWhiteSpace(phoneNumber) && p.PhoneNumber == phoneNumber));
        foreach (var p in pendingRegs)
            _unitOfWork.Repository<PendingRegistration>().Remove(p);

        // 6. Delete OTP Verifications (linked by Identifier = Email/Phone)
        var otpVerifications = await _unitOfWork.Repository<OtpVerification>()
            .FindAsync(o =>
                o.Identifier == email ||
                (!string.IsNullOrWhiteSpace(phoneNumber) && o.Identifier == phoneNumber));
        foreach (var o in otpVerifications)
            _unitOfWork.Repository<OtpVerification>().Remove(o);

        if (citizen != null)
        {
            // 7. Delete Renewal Applications (linked by CitizenRegistryId)
            var renewals = await _unitOfWork.Repository<RenewalApplication>().FindAsync(r => r.CitizenRegistryId == citizen.Id);
            foreach (var r in renewals)
                _unitOfWork.Repository<RenewalApplication>().Remove(r);

            // 8. Delete Traffic Violations (linked by CitizenRegistryId)
            var violations = await _unitOfWork.Repository<TrafficViolation>().FindAsync(v => v.CitizenRegistryId == citizen.Id);
            foreach (var v in violations)
                _unitOfWork.Repository<TrafficViolation>().Remove(v);

            // 9. Delete Driving License Applications (and their uploaded files)
            var drivingApps = await _unitOfWork.Repository<DrivingLicenseApplication>()
                .FindAsync(a => a.CitizenRegistryId == citizen.Id);
            foreach (var a in drivingApps)
            {
                AddIfPresent(filesToDelete, a.PersonalPhotoPath);
                AddIfPresent(filesToDelete, a.EducationalCertificatePath);
                AddIfPresent(filesToDelete, a.IdCardPath);
                AddIfPresent(filesToDelete, a.ResidenceProofPath);
                _unitOfWork.Repository<DrivingLicenseApplication>().Remove(a);
            }

            // 10. Delete Vehicle License Applications (and their uploaded files)
            var vehicleApps = await _unitOfWork.Repository<VehicleLicenseApplication>()
                .FindAsync(a => a.CitizenRegistryId == citizen.Id);
            foreach (var a in vehicleApps)
            {
                AddIfPresent(filesToDelete, a.OwnershipProofPath);
                AddIfPresent(filesToDelete, a.VehicleDataCertificatePath);
                AddIfPresent(filesToDelete, a.IdCardPath);
                AddIfPresent(filesToDelete, a.InsuranceCertificatePath);
                AddIfPresent(filesToDelete, a.CustomClearancePath);
                _unitOfWork.Repository<VehicleLicenseApplication>().Remove(a);
            }

            // 11. Delete Driving Licenses (cascade to DrivingLicense.Applications where applicable)
            var drivingLicenses = await _unitOfWork.Repository<DrivingLicense>()
                .FindAsync(l => l.CitizenRegistryId == citizen.Id);
            foreach (var l in drivingLicenses)
                _unitOfWork.Repository<DrivingLicense>().Remove(l);

            // 12. Delete Vehicle Licenses (cascade to VehicleViolations by convention)
            var vehicleLicenses = await _unitOfWork.Repository<VehicleLicense>()
                .FindAsync(l => l.CitizenRegistryId == citizen.Id);
            foreach (var l in vehicleLicenses)
                _unitOfWork.Repository<VehicleLicense>().Remove(l);
        }

        _logger.LogInformation("Business data cleanup completed for NationalId: {NationalId}", nationalId);
        return filesToDelete.ToList();
    }

    private static void AddIfPresent(HashSet<string> set, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        set.Add(path.Trim());
    }

    private void DeleteFilesBestEffort(IEnumerable<string> paths)
    {
        foreach (var p in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(p)) continue;

                var path = p.Trim();
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete uploaded file: {Path}", p);
            }
        }
    }
}
