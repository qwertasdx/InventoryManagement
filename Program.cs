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

// ���o�n�J�ϥΪ̸�T
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
{
    //���n�J�ɷ|�۰ʾɨ�o�Ӻ��}
    option.LoginPath = new PathString("/User/NoLogin");

    //�S���v���ɡA�|�۰ʾɤJ�����}
    option.AccessDeniedPath = new PathString("/User/NoAccess");
    //option.LoginPath = new PathString("/User/NoAccess");
});

//�Ҧ�api�������Ҥ~�i�ϥ�
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddDbContext<WebContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("WebDatabase")));

//�[�JItemStocksController API
//builder.Services.AddTransient<ItemStocksController>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

//���Ҿ���A���T�����Ǥ��i�A��
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=User}/{action=Login}/{id?}");
    pattern: "{controller=ItemBasics}/{action=Index}/{id?}");
app.Run();
