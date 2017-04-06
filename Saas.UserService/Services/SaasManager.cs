using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Saas.Models;
using Saas.UserService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Saas.UserService.Services
{
    public class SaasManager
    {
         ApplicationDbContext context = ApplicationDbContext.Create();

        public bool AddRole(string roleName,out string result)
        {
            bool isAdded = false;
            result = "Role already Exists!";
            if (!context.Roles.Any(r => r.Name == roleName))
            {
                IdentityRole newRole = new IdentityRole { Name = roleName };
                var resultRole = GetRoleManager().Create(newRole);
                isAdded = resultRole.Succeeded;
                result = !isAdded ? string.Join(",",resultRole.Errors) : newRole.Id ;
            }
            return isAdded;
        }

        private RoleManager<IdentityRole> GetRoleManager()
        {
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);
            return roleManager;
        }

        public void AddUserAndItsRole(string userName, string password, string roleName)
        {
            string roleResult = string.Empty;
            bool userAdd = AddUser(userName, password, out roleResult);
            string newUserId = string.Empty;
            if (userAdd)
            {
                newUserId = roleResult;
                if (AddRole(roleName, out roleResult))
                {
                    AddRolesToUserId(newUserId, new[] { roleResult });
                }

            }            
        }

        private bool AddUser(string userName, string password, out string roleResult)
        {
            bool isAdded = false;
            roleResult = "Not added!";
            
            if (!context.Users.Any(u => u.UserName == userName))
            {
                var user = new ApplicationUser { UserName = userName };
                GetUserManager().Create(user, password);
                isAdded = true;
                roleResult = user.Id;
            }
            else
            {
                var userexists = context.Users.Where(u => u.UserName == userName).FirstOrDefault();
                roleResult = userexists.Id;
                isAdded = true;
            }
            return isAdded;
        }

        private UserManager<ApplicationUser> GetUserManager()
        {
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(store);
            return userManager;
        }

        public ServiceResult AddUserAndItsRoles(NewUserWithRolesId newUserWithIds)
        {
            ServiceResult sr = new ServiceResult();
            sr.isSuccess = false;
            sr.Message = "User can not be added successfully";
                        
            ApplicationUser user = new ApplicationUser { UserName = newUserWithIds.Email, Email = newUserWithIds.Email, PhoneNumber = newUserWithIds.ContactNumber, PhoneNumberConfirmed = true, EmailConfirmed = true };
            var manager = GetUserManager();
            var result = manager.Create(user, newUserWithIds.ContactNumber);
            sr.isSuccess = result.Succeeded;
            sr.Message = !sr.isSuccess ? string.Join(",", result.Errors) : user.Id;

            var roleResult = AddRolesToUserId(user.Id, newUserWithIds.RoleIds);

            return sr;
        }

        public ServiceResult UpdateUserAndItsRolesAndApps( string decrypted, string email, string contactNumber, string[] newRoles,string[] oldRoles,int[] apps)
        {
            ServiceResult sr = new ServiceResult();
            sr.isSuccess = false;
            sr.Message = "User can not be added successfully";

            var manager = GetUserManager();
            var user = manager.FindById(decrypted);            
            user.Email = email;
            user.PasswordHash = manager.PasswordHasher.HashPassword(contactNumber);
            user.UserName = email;
            user.PhoneNumber = contactNumber;            
            var result = manager.Update(user);
            
            sr.isSuccess = result.Succeeded;
            sr.Message = !sr.isSuccess ? string.Join(",", result.Errors) : decrypted;

            if ((oldRoles !=null && oldRoles.Length > 0) && (newRoles != null && newRoles.Length > 0))
            {
                manager.RemoveFromRoles(decrypted, context.Roles.Where(r => oldRoles.Contains(r.Id)).Select(r => r.Name).ToArray());
            }
            if (newRoles != null && newRoles.Length > 0)
                AddRolesToUserId(decrypted, newRoles);
            if (user.SaasApps.Count > 0 && (apps!=null && apps.Length > 0))
            {
                user.SaasApps.Clear();
            }
            AddAppsToUser(apps, user);
            return sr;
        }

        private IdentityResult AddRolesToUserId(string decrypted, string[] newRoles)
        {
            var manager = GetUserManager();
            return manager.AddToRoles(decrypted, context.Roles.Where(r => newRoles.Contains(r.Id)).Select(r => r.Name).ToArray());            
        }

        public bool EditRole(IdentityRole role, out string result)
        {
            bool isAdded = false;
            result = "Role does not exists!";
            if (!context.Roles.Any(r => r.Id == role.Id))
            {
                var resultRole = GetRoleManager().Update(role);
                isAdded = resultRole.Succeeded;
                result = !isAdded ? string.Join(",", resultRole.Errors) : role.Id;
            }
            return isAdded;
        }

        public ServiceResult EditRole(IdentityRole role)
        {
            ServiceResult sr = new ServiceResult();
            sr.isSuccess = false;
            sr.Message = "Role does not exists!";
            if (context.Roles.Any(r => r.Id == role.Id))
            {
                var resultRole = GetRoleManager().Update(role);
                sr.isSuccess = resultRole.Succeeded;
                sr.Message = !sr.isSuccess? string.Join(",", resultRole.Errors) : role.Id;
            }
            return sr;
        }

        public ServiceResult EditApplication(SaasApp updateApp)
        {
            ServiceResult sr = new ServiceResult();
            sr.isSuccess = false;

            context.Entry(updateApp).State = System.Data.Entity.EntityState.Modified;    
            int count = context.SaveChanges();
            sr.isSuccess = count != -1;
            sr.Message = updateApp.SaasAppId.ToString();
            return sr;
        }

        public ServiceResult AddApplication(SaasApp model)
        {
            ServiceResult sr = new ServiceResult();
            sr.isSuccess = false;
            sr.Message = "Failure in saving!";
            var result = context.SaasApps.Add(model);
            int count = context.SaveChanges();
            sr.isSuccess = count != -1;
            sr.Message =  result.SaasAppId.ToString();
            return sr;
        }

        public ServiceResult AddRole(IdentityRole newRole)
        {
            ServiceResult sr = new ServiceResult();
            sr.isSuccess = false;
            sr.Message = "Role already Exists!";

            if (!context.Roles.Any(r => r.Name == newRole.Name))
            {
                var resultRole = GetRoleManager().Create(newRole);
                sr.isSuccess = resultRole.Succeeded;
                sr.Message  = !sr.isSuccess? string.Join(",", resultRole.Errors) : newRole.Id;
            }
            return sr;
        }

        public async Task<IdentityResult> DeleteRoleAsync(string id)
        {
            var manager = GetRoleManager();
            var role = manager.FindById(id);
           return await manager.DeleteAsync(role);
        }

        public ServiceResult DeleteUser(string id)
        {
            var manager = GetUserManager();
            var user = manager.FindById(id);
            var result = manager.Delete(user);

            ServiceResult sr = new ServiceResult();
            sr.isSuccess = result.Succeeded;
            sr.Message = !sr.isSuccess ? string.Join(",", result.Errors) : id;

            return sr;
        }

        public void DeleteApplication(int id)
        {
            var app = context.SaasApps.Find(new object[] { id });
            app.IsActive = false;
            context.SaveChanges();

        }

        public ServiceResult AddUserAndItsRolesAndApps(string Email, string ContactNumber, string[] RoleIds, int[] AppIds)
        {
            ServiceResult sr = new ServiceResult();
            sr.isSuccess = false;
            sr.Message = "User can not be added successfully";

            ApplicationUser user = new ApplicationUser { UserName = Email, Email = Email, PhoneNumber = ContactNumber, PhoneNumberConfirmed = true, EmailConfirmed = true };
            var manager = GetUserManager();
            var result = manager.Create(user, ContactNumber);
            sr.isSuccess = result.Succeeded;
            sr.Message = !sr.isSuccess ? string.Join(",", result.Errors) : user.Id;

            if (RoleIds != null && RoleIds.Length > 0)
            {
                var roleResult = AddRolesToUserId(user.Id, RoleIds);
                sr.Message = sr.isSuccess && !roleResult.Succeeded ? string.Join(",", result.Errors): sr.Message;
                sr.isSuccess = sr.isSuccess ? roleResult.Succeeded : sr.isSuccess;
            }
            AddAppsToUser(AppIds, user);
            return sr;
        }

        private void AddAppsToUser(int[] AppIds, ApplicationUser user)
        {
            if (AppIds != null && AppIds.Length > 0)
            {
                context.SaasApps.Where(app => AppIds.Contains(app.SaasAppId)).ToList().ForEach(app => user.SaasApps.Add(app));
                context.SaveChanges();
                //var appResult =  AddAppsToUserId(user.Id, AppIds);
            }
        }

        private int AddAppsToUserId(string id, int[] appIds)
        {
            return -1;
        }
    }
}