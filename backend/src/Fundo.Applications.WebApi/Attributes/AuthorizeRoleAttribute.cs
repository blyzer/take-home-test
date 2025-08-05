using Microsoft.AspNetCore.Authorization;

namespace Fundo.Applications.WebApi.Attributes
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
    
    public class AdminOnlyAttribute : AuthorizeRoleAttribute
    {
        public AdminOnlyAttribute() : base("Admin") { }
    }
    
    public class ManagerOrAdminAttribute : AuthorizeRoleAttribute
    {
        public ManagerOrAdminAttribute() : base("Manager", "Admin") { }
    }
    
    public class AuthenticatedUserAttribute : AuthorizeAttribute
    {
        public AuthenticatedUserAttribute() 
        {
            // Any authenticated user
        }
    }
}
