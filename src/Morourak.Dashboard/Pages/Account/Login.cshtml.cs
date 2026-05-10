using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morourak.Application.DTOs.Auth;
using Morourak.Dashboard.Services;
using System.Security.Claims;

namespace Morourak.Dashboard.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var request = new LoginRequestDto
            {
                MobileNumber = Input.MobileNumber,
                Password = Input.Password
            };

            var result = await _authService.LoginAsync(request);

            if (result != null && result.IsSuccess && !string.IsNullOrEmpty(result.Token))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, result.FullName ?? result.Username ?? Input.MobileNumber),
                    new Claim("JWToken", result.Token)
                };

                if (result.Roles != null)
                {
                    foreach (var role in result.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Redirect based on role
                if (result.Roles != null)
                {
                    if (result.Roles.Contains("ADMIN")) return RedirectToPage("/Admin/Index");
                    if (result.Roles.Contains("DOCTOR") || 
                        result.Roles.Contains("EXAMINATOR") || 
                        result.Roles.Contains("INSPECTOR")) 
                    {
                        return RedirectToPage("/Staff/Appointments");
                    }
                }

                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, result?.Message ?? "فشل تسجيل الدخول. يرجى المحاولة مرة أخرى.");
            return Page();
        }
    }

    public class LoginInputModel
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
