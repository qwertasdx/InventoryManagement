using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.Dto;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;


namespace InventoryManagement.Controllers
{
    [AllowAnonymous] //不需做驗證即可使用
    public class UserController : Controller
    {
        private readonly WebContext _context;
        public UserController(WebContext context)
        {
            _context = context;      
        }

        // GET: Login1
        public  IActionResult Login()
        {
            return View();
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginPost value)
        {
            if (ModelState.IsValid)
            {
                var user = (from a in _context.User
                            where a.Account == value.Account
                            && a.Password == value.Password
                            select a).SingleOrDefault();

                if (user == null)
                {
                    ModelState.AddModelError("Password", "你輸入的帳號或密碼錯誤");                   
                    return View();
                }
                else
                {
                    //驗證
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Account),
                        new Claim("FullName", user.EmployeeName),
                        new Claim("EmployeeId", user.EmployeeId),
                    };

                    // 將角色塞入Claim
                    var role = from a in _context.User
                               where a.EmployeeId == user.EmployeeId
                               select a;

                    foreach(var tmp in role)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, tmp.Role.Trim()));
                    }
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
              
                    return RedirectToAction("Index", "ItemBasics");
                }
            }
            return View();
        }

       
        [HttpGet]
        public IActionResult logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public IActionResult noLogin()
        {
            return View();
        }

        [HttpGet]
        public IActionResult noAccess()
        {
            return View();
        }

        // GET: Login/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Login/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
        
            var result =  (from a in _context.User
                           where a.EmployeeId == user.EmployeeId
                           select a).SingleOrDefault();


            // 員工已建立帳號
            if (result != null)
            {
                ModelState.AddModelError("EmployeeId", "你已建立過帳號");
                return View();
            }

            var result2 = (from b in _context.User
                           where b.Account == user.Account
                           select b).FirstOrDefault();

            if (result2 != null)
            {
                ModelState.AddModelError("Account", "帳號已使用，請重新輸入");
                return View();
            }

            user.EmployeeId = user.EmployeeId.ToUpper();
            user.Role="employee";
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Login));                
        }

        // GET: Login1/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Login1/Edit/5
        // 防止跨網站偽造請求的攻擊
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("EmployeeId,EmployeeName,Account,Password")] User user)
        {
            if (id != user.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

            
        // GET: Login1/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // POST: Login1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.User.Any(e => e.EmployeeId == id);
        }
    }
}
