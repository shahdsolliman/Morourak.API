using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Morourak.Infrastructure.Identity
{
    public class IdentityDbContext: IdentityDbContext<ApplicationUser>
    {
        public IdentityDbContext(
            DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }
    }
}
