using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ITInternHub.Models;
using ITInternHub.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối SQL Server
builder.Services.AddDbContext<ITInternHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });


//Đăng ký Repositories (DIP: Interface -> Implementation)
builder.Services.AddScoped<ITInternHub.Core.Application.Contracts.Repositories.IJobPostRepository,
                           ITInternHub.Infrastructure.Persistence.Repositories.JobPostRepository>();

// 2. Đăng ký các Use Cases
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.ApproveJobUseCase>();
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.RejectJobUseCase>();
// 3. Đăng ký Dịch vụ
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddControllersWithViews();

// Đăng ký HttpContextAccessor để lấy IP máy khách
builder.Services.AddHttpContextAccessor();

// Đăng ký Dịch vụ Audit Log
builder.Services.AddScoped<IAuditService, AuditService>();
// Đăng ký Use Case tạo tin tuyển dụng
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.CreateJobPostUseCase>();

// Đăng ký Use Case cập nhật bài đăng
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.UpdateJobPostUseCase>();

// ==============================================================
// ĐĂNG KÝ REPOSITORIES VÀ USE CASES THEO CHUẨN CLEAN ARCHITECTURE
// ==============================================================

// 1. Repositories
builder.Services.AddScoped<ITInternHub.Core.Application.Contracts.Repositories.IJobPostRepository,
                           ITInternHub.Infrastructure.Persistence.Repositories.JobPostRepository>();

builder.Services.AddScoped<ITInternHub.Core.Application.Contracts.Repositories.ISkillRepository,
                           ITInternHub.Infrastructure.Persistence.Repositories.SkillRepository>();

// 2. Use Cases Duyệt tin (Đơn lẻ & Hàng loạt)
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.ApproveJobUseCase>();
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.RejectJobUseCase>();
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.BulkApproveJobsUseCase>();
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Jobs.BulkRejectJobsUseCase>();

// 3. Use Cases Quản trị Kỹ năng
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Skills.CreateSkillUseCase>();
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Skills.UpdateSkillUseCase>();
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Skills.DeleteSkillUseCase>();
builder.Services.AddScoped<ITInternHub.Core.Application.UseCases.Skills.ToggleSkillHotUseCase>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. Bật Authentication trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// Route cho Area (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Route Mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.Run();