using AppAPI.IServices;
using AppAPI.Services;
using AppData.Models;
using AppData.ViewModels.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// CORS policy
var AllowSpecificOrigins = "_allowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ AddControllers gộp 1 lần duy nhất, cấu hình đầy đủ
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Example API",
        Version = "v1",
        Description = "An example of an ASP.NET Core Web API",
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Email = "example@example.com",
            Url = new Uri("https://example.com/contact"),
        },
    });
});

// DbContext
builder.Services.AddDbContext<AssignmentDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBContext")));

// DI Services
builder.Services.AddScoped<IChiTietGioHangServices, ChiTietGioHangServices>();
builder.Services.AddScoped<IGioHangServices, GioHangServices>();
builder.Services.AddScoped<IQuyDoiDiemServices, QuyDoiDiemServices>();
builder.Services.AddScoped<IKhuyenMaiServices, KhuyenMaiServices>();
builder.Services.AddScoped<IHoaDonService, HoaDonService>();
builder.Services.AddScoped<IKhachHangService, KhachHangService>();
builder.Services.AddScoped<ILishSuTichDiemServices, LishSuTichDiemServices>();
builder.Services.AddScoped<ILoaiSPService, LoaiSPService>();
builder.Services.AddScoped<INhanVienService, NhanVienService>();
builder.Services.AddScoped<IQuanLyNguoiDungService, QuanLyNguoiDungService>();
builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<IVoucherServices, VoucherServices>();
builder.Services.AddScoped<IThongKeService, ThongKeService>();
builder.Services.AddScoped<IVaiTroService, VaiTroSevice>();
builder.Services.AddScoped<AssignmentDBContext>();
builder.Services.AddHttpClient<GHNService>();

// Mail settings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IMailServices, MailServices>();
builder.Services.AddTransient<IMailServices, MailServices>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddDataProtection();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(cfg =>
{
    cfg.IdleTimeout = TimeSpan.FromHours(1);
    cfg.Cookie.HttpOnly = true;
    cfg.Cookie.IsEssential = true;
});

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

// Middlewares
app.UseHttpsRedirection();
app.UseCors(AllowSpecificOrigins);
app.UseAuthorization();
app.UseSession();
app.MapControllers();
app.Run();
