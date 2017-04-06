using Saas.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Saas.UserManage.Models
{
    public class UserView
    {
        public string Id { get; set; }
        public string Roles { get; set; }
        [Required]
        [EmailAddress]
        //[Remote("EmailExists","ManageAdmin",ErrorMessage = "Email already exists!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //[Required]
        //[EmailAddress]
        //[Display(Name = "Email")]
        //public string EditEmail { get; set; }
        [Required]
        [Phone]   
        [Display(Name = "Mobile Number")]
        public string ContactNumber { get; set; }
        public List<RoleView> UserRoles { get; set; }
        public int[] Apps { get; set; }
        public IEnumerable<SelectListItem> AppsSelect { get; set; }
    }
}