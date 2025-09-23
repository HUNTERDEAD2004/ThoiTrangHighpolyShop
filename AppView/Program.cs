using AppData.Models;
using AppView.IServices;
using AppView.Models.Momo;
using AppView.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Momo API Payment
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoOption"));
builder.Services.AddScoped<IMomoService, MomoService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7095/api/");
    client.Timeout = TimeSpan.FromMinutes(2); // timeout rõ ràng
});

// ? Thêm AssignmentDBContext
builder.Services.AddDbContext<AssignmentDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBContext")));

builder.Services.AddSession(cfg =>
{
    cfg.IdleTimeout = new TimeSpan(1,0,0);
    cfg.Cookie.HttpOnly = true;
    cfg.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}"
);

IWebHostEnvironment env = app.Environment;
Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath, "../Rotativa/Windows");
app.Run();
