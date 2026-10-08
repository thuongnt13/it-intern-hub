using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITInternHub.Models;

namespace ITInternHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Cho phép cả Admin và Cán bộ Nhà trường cùng truy cập vào Dashboard
    [Authorize(Roles = "ROLE_ADMIN,ROLE_UNIVERSITY")]
    public class DashboardController : Controller
    {
        private readonly ITInternHubDbContext _context;

        public DashboardController(ITInternHubDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Số tin tuyển dụng chờ duyệt (Chỉ Admin quan tâm, nhưng vẫn đếm)
            ViewBag.PendingJobsCount = await _context.JobPosts.CountAsync(j => j.Status == 1);

            // 2. Thống kê sinh viên
            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ROLE_STUDENT");
            ViewBag.TotalStudents = studentRole != null
                ? await _context.UserRoles.CountAsync(ur => ur.RoleId == studentRole.Id)
                : 0;

            // 3. Thống kê số doanh nghiệp
            var employerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ROLE_EMPLOYER");
            ViewBag.TotalCompanies = employerRole != null
                ? await _context.UserRoles.CountAsync(ur => ur.RoleId == employerRole.Id)
                : 0;

            // 4. Số đơn ứng tuyển
            ViewBag.TotalApplications = await _context.Applications.CountAsync();

            // 5. Top kỹ năng các doanh nghiệp đang tuyển nhiều nhất (Báo cáo thực tế cho Nhà trường)
            var topSkills = await _context.JobSkills
                .Include(js => js.Skill)
                .GroupBy(js => js.Skill.Name)
                .Select(g => new { SkillName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.TopSkills = topSkills;

            return View();
        }
    }
}