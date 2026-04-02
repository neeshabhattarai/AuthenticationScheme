using Microsoft.AspNetCore.Identity;

namespace AuthenticationScheme.Infastructure.Entities;

public class User:IdentityUser
{
    public string Department { get; set; }
}