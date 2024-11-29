using InventoryManagement.Controllers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GlobalSettings>();

builder.Services.AddAutoMapper(typeof(Program));
// Add services to the container.
builder.Services.AddControllersWithViews();

// 取得登入使用者資訊
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
{
    //未登入時會自動導到這個網址
    option.LoginPath = new PathString("/User/NoLogin");

    //沒有權限時，會自動導入此網址
    option.AccessDeniedPath = new PathString("/User/NoAccess");
    //option.LoginPath = new PathString("/User/NoAccess");
});

//所有api都需驗證才可使用
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddDbContext<WebContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("WebDatabase")));

//加入ItemStocksController API
//builder.Services.AddTransient<ItemStocksController>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

//驗證機制，此三項順序不可顛倒
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=User}/{action=Login}/{id?}");
    pattern: "{controller=ItemBasics}/{action=Index}/{id?}");
app.Run();
