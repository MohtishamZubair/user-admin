using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Saas.Models;
using Saas.UserManage.Helper;
using Saas.UserManage.Models;
using Saas.UserService.Models;
using Saas.UserService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Saas.UserManage.Controllers
{
    [Authorize(Roles = "admin")]
    public class ManageAdminController : Controller
    {
        string EditMessage = "Edit {0}";
        string CreateMessage = "Create new {0}";

        ApplicationDbContext _context = ApplicationDbContext.Create();

        public ActionResult Index()
        {
            return RedirectToAction("Applications");
        }

        public ActionResult Roles()
        {
            return loadAll();
        }

        public ActionResult Applications()
        {
            return loadAllApplications();
        }

        private ActionResult loadAllApplications(string newlyAdded = "", string message = "")
        {
            var all = _context.SaasApps.ToList();
            return LoadAllForm("Applications", newlyAdded, message, () => { return all; });
        }

        private ActionResult loadAll(string newlyAdded = "", string message = "")
        {
            var roles = _context.Roles.Select(r => new RoleView { Id = r.Id, Name = r.Name });
            return LoadAllForm("Roles", newlyAdded, message, () => { return roles; });
        }

        private ActionResult loadAllUsers(string newlyAdded = "", string message = "")
        {
            var roles = _context.Roles.ToList();
            var saasUsers = _context.Users.Where(u => u.UserName != Helper.AppConstant.ADMIN_USER_NAME).ToList().Select(
                u => new UserView
                {
                    Id = WebHelper.Encrypt(u.Id),
                    Email = u.Email,
                    ContactNumber = u.PhoneNumber,
                    Roles = string.Join(",", (u.Roles.Join(roles, ur => ur.RoleId, apr => apr.Id, (ur, apr) => (apr.Name))).ToArray())
                }
            );
            return LoadAllForm("Users", WebHelper.Encrypt(newlyAdded), message, () => { return saasUsers; });
        }

        private ActionResult LoadAllForm(string formName, string newlyAdded, string message, Func<IEnumerable<object>> allObjects)
        {
            if (!string.IsNullOrWhiteSpace(newlyAdded))
            {
                ViewBag.newlyAdded = newlyAdded;
                ViewBag.message = message;
            }
            return View(formName, allObjects());
        }


        public ActionResult EditRoles(string Id)
        {
            var role = _context.Roles.First(r => r.Id == Id);
            if (role == null)
            {
                return HttpNotFound();
            }
            var rv = new RoleView { Name = role.Name, Id = role.Id };
            return LoadForm(rv, EditMessage);
        }

        public ActionResult EditApplication(int SaasAppId)
        {
            var app = _context.SaasApps.Where(ap => ap.SaasAppId == SaasAppId).SingleOrDefault();
            return LoadAppForm(app, EditMessage);
        }

        public ActionResult EditUser(string Id)
        {
            string decrypted = WebHelper.Decrypt(Id);
            var user = _context.Users.First(u => u.Id == decrypted);
            var userRoles = user.Roles.ToList();
            if (user == null)
            {
                return HttpNotFound();
            }
            var usertoEdit = new UserView { Email = user.Email, Id = Id, ContactNumber = user.PhoneNumber };
            usertoEdit.UserRoles = _context.Roles.ToList().Select(
                r => new RoleView
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsChecked = userRoles.Any(ur => ur.RoleId == r.Id)
                }).ToList();

            return LoadEditForm(user, usertoEdit);            
        }

        private ActionResult LoadEditForm(ApplicationUser user, UserView usertoEdit)
        {
            var UserApps = user.SaasApps.Select(ap => ap.SaasAppId).ToArray();
            ViewBag.AppList = _context.SaasApps.Where(a => a.IsActive).ToList().Select(app => new SelectListItem { Text = app.Name, Value = app.SaasAppId.ToString(), Selected = UserApps.Contains(app.SaasAppId) }).ToList();
            return LoadUserForm(usertoEdit, EditMessage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(UserView model)
        {
            string decrypted = WebHelper.Decrypt(model.Id);
            var user = _context.Users.First(u => u.Id == decrypted);

            if (user == null)
            {
                return HttpNotFound();
            }

            return SaveAction(string.Format(" User with email {0} updated successfully!", model.Email)
                , () => { return new SaasManager().UpdateUserAndItsRolesAndApps(decrypted, model.Email, model.ContactNumber, model.UserRoles.Where(r => r.IsChecked).Select(r => r.Id).ToArray(), user.Roles.Select(r => r.RoleId).ToArray(),model.Apps); }
                , () => { return LoadEditForm(user, model); }
                , loadAllUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRoles(RoleView model)
        {
            var role = _context.Roles.First(r => r.Id == model.Id);
            if (role == null)
            {
                return HttpNotFound();
            }
            var updateRole = new IdentityRole { Id = model.Id, Name = model.Name };
            return SaveAction(string.Format(" {0} role updated successfully!", model.Name), () => { return new SaasManager().EditRole(updateRole); }, () => { return LoadForm(model, EditMessage); }, loadAll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditApplication(SaasApp model)
        {
            var app = _context.SaasApps.Where(ap => ap.SaasAppId == model.SaasAppId).SingleOrDefault();
            if (app == null)
            {
                return HttpNotFound();
            }
            var updateApp = new SaasApp{ SaasAppId = model.SaasAppId, Name = model.Name,Code=model.Code,IsActive=model.IsActive };
            return SaveAction(string.Format(" {0} role updated successfully!", model.Name), () => { return new SaasManager().EditApplication(updateApp); }, () => { return LoadAppForm(model, EditMessage); }, loadAllApplications);
        }

        public ActionResult CreateRoles()
        {
            RoleView rv = new RoleView();
            return LoadForm(rv, CreateMessage);
        }

        public ActionResult CreateUser()
        {
            UserView newUser = new UserView();
            newUser.UserRoles = _context.Roles.Select(r => new RoleView { Id = r.Id, Name = r.Name, IsChecked = false }).ToList();
            ViewBag.AppList = _context.SaasApps.Where(a => a.IsActive).ToList().Select(app => new SelectListItem { Text = app.Name, Value = app.SaasAppId.ToString() }).ToList();            
            return LoadUserForm(newUser, CreateMessage);
        }

        public ActionResult CreateApplication()
        {
            SaasApp newApp = new SaasApp();
            return LoadAppForm(newApp, CreateMessage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRoles(RoleView model)
        {
            var newRole = new IdentityRole { Name = model.Name };
            return SaveAction(string.Format(" {0} role added successfully!", model.Name), () => { return new SaasManager().AddRole(newRole); }, () => { return LoadForm(model, CreateMessage); }, loadAll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateApplication(SaasApp model)
        {
            return SaveAction(string.Format(" Application with Name {0} added successfully!", model.Name), () => { return new SaasManager().AddApplication(model); }, () => { return LoadAppForm(model, CreateMessage); }, loadAllApplications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(UserView model)
        {
            var selectedRoleids = model.UserRoles.Where(r => r.IsChecked).Select(r => r.Id).ToArray();
            //var newUserWithIds = new NewUserWithRolesId { Email = model.Email, ContactNumber = model.ContactNumber, RoleIds = selectedRoleids };
            ViewBag.AppList = _context.SaasApps.Where(a => a.IsActive).ToList().Select(app => new SelectListItem { Text = app.Name, Value = app.SaasAppId.ToString() , Selected = model.Apps.Contains(app.SaasAppId) }).ToList();
            return SaveAction(string.Format(" User with email {0} added successfully!", model.Email), () => { return new SaasManager().AddUserAndItsRolesAndApps(Email : model.Email, ContactNumber : model.ContactNumber, RoleIds : selectedRoleids, AppIds : model.Apps); }, () => { return LoadUserForm(model, CreateMessage); }, loadAllUsers);
        }

        private ActionResult LoadAppForm(SaasApp app, string actionName)
        {
            ViewBag.ViewAction = string.Format(actionName, "application");
            return View("ApplicationForm", app);
        }

        private ActionResult LoadUserForm(UserView uv, string actionName)
        {
            ViewBag.ViewAction = string.Format(actionName, "user");
            return View("LoadUserForm", uv);
        }

        private ActionResult LoadForm(RoleView rv, string v)
        {
            ViewBag.ViewAction = string.Format(v, "role");
            return View("LoadForm", rv);
        }

        public ActionResult SaveAction(string successMessage, Func<ServiceResult> saveAction, Func<ActionResult> onError, Func<string, string, ActionResult> loadAllFunc)
        {
            if (!ModelState.IsValid)
            {
                return onError();
            }

            ServiceResult serviceResult = saveAction();

            if (!serviceResult.isSuccess)
            {
                ModelState.AddModelError("", serviceResult.Message);
                return onError();
            }
            return loadAllFunc(serviceResult.Message, successMessage);
        }

        public ActionResult Users()
        {
            return loadAllUsers();
        }

        public ActionResult DeleteApplication(string id, string name)
        {
            return LoadDeleteForm(id, name, "application", () => { return _context.SaasApps.Find(new object[] { int.Parse(id) }); });
        }

        private ActionResult LoadDeleteForm(string id, string name, string typeName, Func<object> nullCheck)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var nullObject = nullCheck();
            if (nullObject == null)
            {
                return HttpNotFound();
            }
            var deleteView = new DeleteView { Id = id, Name = name, TypeName = typeName };
            return View("DeleteView", deleteView);
        }

        public ActionResult DeleteRole(string id, string name)
        {
            return LoadDeleteForm(id, name, "role", () => { return _context.Roles.Find(new[] { id }); });
        }

        public ActionResult DeleteUser(string id, string name)
        {
            return LoadDeleteForm(id, name, "user", () => { return _context.Users.Find(new[] { WebHelper.Decrypt(id) }); });
        }

        [HttpPost, ActionName("DeleteApplication")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteApplication(int id)
        {
            new SaasManager().DeleteApplication(id);
            return RedirectToAction("Applications");
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUserConfirmed(string id)
        {
            new SaasManager().DeleteUser(WebHelper.Decrypt(id));
            return RedirectToAction("Users");
        }

        [HttpPost, ActionName("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            await new SaasManager().DeleteRoleAsync(id);
            return RedirectToAction("Roles");
        }

        public ActionResult RoleExists(String Name)
        {
            return Json(!_context.Roles.Any(r => r.Name == Name), JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmailExists(String Email)
        {
            return Json(!_context.Users.Any(r => r.Email == Email), JsonRequestBehavior.AllowGet);
        }
    }
}
