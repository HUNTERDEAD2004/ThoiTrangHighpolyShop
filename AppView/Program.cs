using AppData.Models;
using AppView.IServices;
using AppView.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AssignmentDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBContext")));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSession(cfg =>
{
    cfg.IdleTimeout = new TimeSpan(1,0,0);
});

builder.Services.AddControllers().AddJsonOptions(options =>
{

    options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
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
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}"
);

IWebHostEnvironment env = app.Environment;
Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath, "../Rotativa/Windows");
app.Run();
