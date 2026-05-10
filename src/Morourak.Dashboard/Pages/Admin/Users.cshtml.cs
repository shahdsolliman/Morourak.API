using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Application.DTOs.Admin;
using Morourak.Dashboard.Services;

namespace Morourak.Dashboard.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class UsersManagementModel : PageModel
    {
        private readonly IAdminService _adminService;

        public UsersManagementModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public List<UserDto> Users { get; set; } = new();
        public int TotalUsersCount { get; set; }
        public int DoctorsCount { get; set; }
        public int InspectorsCount { get; set; }
        public int ExaminatorsCount { get; set; }
        public int CitizensCount { get; set; }
        public int ActiveUsersCount { get; set; }

        [BindProperty]
        public CreateUserDto CreateDto { get; set; } = new();

        [BindProperty]
        public UpdateUserDto UpdateDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var usersResult = await _adminService.GetUsersAsync(1, 100);
                
                if (usersResult != null && usersResult.IsSuccess)
                {
                    Users = usersResult.Details?.ToList() ?? new List<UserDto>();
                    TotalUsersCount = usersResult.TotalRecords;
                    
                    DoctorsCount = Users.Count(u => string.Equals(u.Role, "DOCTOR", StringComparison.OrdinalIgnoreCase));
                    InspectorsCount = Users.Count(u => string.Equals(u.Role, "INSPECTOR", StringComparison.OrdinalIgnoreCase));
                    ExaminatorsCount = Users.Count(u => string.Equals(u.Role, "EXAMINATOR", StringComparison.OrdinalIgnoreCase));
                    CitizensCount = Users.Count(u => string.Equals(u.Role, "CITIZEN", StringComparison.OrdinalIgnoreCase));
                    ActiveUsersCount = Users.Count(u => u.IsActive);
                    
                    if (TotalUsersCount < Users.Count) TotalUsersCount = Users.Count;
                }
                else
                {
                    Users = new List<UserDto>();
                    TotalUsersCount = 0;
                    TempData["ErrorMessage"] = usersResult?.Message ?? "فشل في جلب قائمة المستخدمين من الخادم.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[USERS ERROR] {ex.Message}");
                Users = new List<UserDto>();
                TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء الاتصال بالخادم.";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return Page();

            var result = await _adminService.DeleteUserAsync(id);
            if (result != null && result.IsSuccess)
            {
                TempData["SuccessMessage"] = "تم حذف المستخدم بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = result?.Message ?? "فشل في حذف المستخدم.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _adminService.CreateUserAsync(CreateDto);
            if (result != null && result.IsSuccess)
            {
                TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح.";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = result?.Message ?? "فشل في إنشاء المستخدم.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return Page();

            var result = await _adminService.UpdateUserAsync(id, UpdateDto);
            if (result != null && result.IsSuccess)
            {
                TempData["SuccessMessage"] = "تم تحديث بيانات المستخدم بنجاح.";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = result?.Message ?? "فشل في تحديث بيانات المستخدم.";
            return RedirectToPage();
        }
    }
}
