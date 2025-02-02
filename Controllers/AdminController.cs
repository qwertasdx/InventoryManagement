using InventoryManagement.Dto;
using InventoryManagement.Models;
using InventoryManagement.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventoryManagement.Controllers
{
    [Authorize(Roles = "admin")]  // 加上此段，代表只有驗證為管理人員才可使用
    public class AdminController : Controller
    {
        private readonly WebContext _context;
        private readonly GlobalSettings _globalSettings;
        public AdminController(WebContext context, GlobalSettings globalSettings)
        {
            _context = context;
            _globalSettings = globalSettings;
        }

        // 顯示員工帳號與身分
        public async Task<IActionResult> UserInfos(string employeeId)
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            var query = from a in _context.User
                         select new Users
                         {
                             EmployeeId = a.EmployeeId,
                             EmployeeName = a.EmployeeName,
                             Account = a.Account,
                             Role = a.Role
                         };
            
            if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(x=>x.EmployeeId == employeeId);
            }

            var result = await query.ToListAsync();

            foreach (var item in result)
            {
                if (item.Role.Trim() == "admin")
                {
                    item.RoleName = "管理人員";
                }
                else
                {
                    item.RoleName = "一般人員";
                }
            }
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string employeeId)
        {
            TempData["EmployeeName"] = _globalSettings.employeeName;
            var UsersViewModel = new UsersViewModel();

            UsersViewModel.Users = await(from a in _context.User
                                   where a.EmployeeId == employeeId
                                   select new Users 
                                   { 
                                       EmployeeId = a.EmployeeId,
                                       EmployeeName = a.EmployeeName,
                                       Account = a.Account,
                                       Role = a.Role
                                   }).SingleOrDefaultAsync();

            ViewBag.selectRole = UsersViewModel.Users.Role.Trim();

            return View(UsersViewModel);
        }

        // POST: AdminController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string roleName, string employeeId)
        {
            var update = await _context.User.FindAsync(employeeId);

            if (update != null && update.Role.Trim()!= roleName)
            {
                update.Role = roleName;
                await _context.SaveChangesAsync();
            }
          
            return RedirectToAction(nameof(UserInfos));
        }


        // 刪除員工帳密
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string employeeId)
        {
            var user =  await _context.User.FindAsync(employeeId.Trim());
            if (user != null)
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
            }
   
            return RedirectToAction(nameof(UserInfos));
        }
    }
}
