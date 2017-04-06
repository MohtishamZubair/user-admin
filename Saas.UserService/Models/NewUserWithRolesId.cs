namespace Saas.UserService.Models
{
    public class NewUserWithRolesId
    {
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string[] RoleIds { get; set; }
    }
}