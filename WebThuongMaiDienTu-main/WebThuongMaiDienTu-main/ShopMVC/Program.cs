using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShopMVC.Data;
using ShopMVC.Middlewares;
using ShopMVC.Models;
using ShopMVC.Services;
using ShopMVC.Services.Interfaces;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using OpenApiInfo = Microsoft.OpenApi.Models.OpenApiInfo;
using OpenApiContact = Microsoft.OpenApi.Models.OpenApiContact;
using OpenApiLicense = Microsoft.OpenApi.Models.OpenApiLicense;
using OpenApiSecurityScheme = Microsoft.OpenApi.Models.OpenApiSecurityScheme;
using ParameterLocation = Microsoft.OpenApi.Models.ParameterLocation;
using SecuritySchemeType = Microsoft.OpenApi.Models.SecuritySchemeType;
using OpenApiSecurityRequirement = Microsoft.OpenApi.Models.OpenApiSecurityRequirement;
using OpenApiReference = Microsoft.OpenApi.Models.OpenApiReference;
using ReferenceType = Microsoft.OpenApi.Models.ReferenceType;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/shopmvc-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(warnings =>
               warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

// DÒNG BỊ LỖI ĐÃ BỊ XÓA Ở ĐÂY
builder.Services.AddSignalR();
// Identity + Roles (Tất cả cấu hình gộp lại trong khối này)
builder.Services
    .AddIdentity<NguoiDung, IdentityRole>(opt => // <-- ĐĂNG KÝ USER VÀ ROLE CÙNG LÚC
    {
        // Cấu hình Password
        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequiredLength = 6;

        // Cấu hình User
        opt.User.RequireUniqueEmail = true;

        // Tùy chọn Sign In (được chuyển từ AddDefaultIdentity xuống)
        opt.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>() // <== ĐĂNG KÝ STORE CHO CẢ USER VÀ ROLE
    .AddDefaultTokenProviders()
    .AddErrorDescriber<VietnameseIdentityErrorDescriber>()
    .AddDefaultUI();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddMemoryCache();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecific", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "http://localhost:5018", "https://localhost:7032")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShopMVC eCommerce API",
        Version = "v1.0",
        Description = "API Documentation for ShopMVC - Professional eCommerce Platform",
        Contact = new OpenApiContact
        {
            Name = "ShopMVC Team",
            Email = "support@shopmvc.local"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });

    // Add security scheme for Bearer token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddRazorPages();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.Cookie.Name = ".ShopMVC.Session";
    opt.IdleTimeout = TimeSpan.FromHours(2);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

var app = builder.Build();
var isDocker = app.Environment.IsEnvironment("Docker");

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable CORS
app.UseCors("AllowSpecific");

// Rate Limiting Middleware
app.UseRateLimiting();

// Swagger - Enabled for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopMVC API v1.0");
    c.RoutePrefix = "api-docs"; // Access at /api-docs
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelExpandDepth(2);
});

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next();
});

if (!app.Environment.IsDevelopment() && !isDocker)
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!isDocker)
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ShopMVC.Hubs.ChatHub>("/chatHub");
app.UseSession();

// Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ====== SEED ROLES + GÁN ADMIN ======
using (var scope = app.Services.CreateScope())
{
    var sv = scope.ServiceProvider;

    // 1) migrate + seed dữ liệu cũ (nếu m đang dùng)
    await DbSeeder.SeedAsync(sv);

    // 2) seed roles + add admin
    var roleMgr = sv.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = sv.GetRequiredService<UserManager<NguoiDung>>();

    // Đổi Admin thành QuanTri (để đồng bộ với DbSeeder) nếu cần, hoặc ngược lại
    string[] roles = new[] { "QuanTri", "Staff" }; // Hoặc "Admin" tùy vào hệ thống bạn dùng

    foreach (var r in roles)
    {
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));
    }

    var adminEmail = "admin@shopmvc.local";
    var adminUser = await userMgr.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new NguoiDung
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            // Thêm HoTen để đồng bộ với DbSeeder
            HoTen = "Quan Tri Vien"
        };
        var create = await userMgr.CreateAsync(adminUser, "Admin@123");
        if (!create.Succeeded)
        {
            throw new Exception("Tạo tài khoản admin thất bại: " +
                string.Join("; ", create.Errors.Select(e => e.Description)));
        }
    }

    // Thêm role Admin (hoặc QuanTri) nếu chưa có
    if (!await userMgr.IsInRoleAsync(adminUser, "QuanTri")) // <== Sử dụng tên role đã tạo
        await userMgr.AddToRoleAsync(adminUser, "QuanTri");
}
// ====== END SEED ======

app.Run();
