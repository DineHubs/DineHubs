using Microsoft.AspNetCore.Identity;

namespace OrderManagement.Identity.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string name) : base(name)
    {
    }
}


