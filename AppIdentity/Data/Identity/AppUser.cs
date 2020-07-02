using Microsoft.AspNetCore.Identity;

namespace AppIdentity.Data.Identity
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
    }
}