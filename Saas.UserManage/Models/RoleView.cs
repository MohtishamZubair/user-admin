using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Saas.UserManage.Models
{
    public class RoleView
    {
        public string Id { get; set; }

        [Required]
        [Remote("RoleExists","ManageAdmin",ErrorMessage ="Role already exists")]
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}