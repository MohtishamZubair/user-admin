using System.Collections.Generic;

namespace Saas.Models
{
    public class SaasApp
    {
        public int SaasAppId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<ApplicationUser> SaasUsers { get; set; }
        
    }
}