using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Morourak.Dashboard.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Account/Login");
        }

        if (User.IsInRole("ADMIN")) return RedirectToPage("/Admin/Index");
        if (User.IsInRole("DOCTOR") || User.IsInRole("EXAMINATOR") || User.IsInRole("INSPECTOR")) 
            return RedirectToPage("/Staff/Appointments");

        return Page();
    }
}
