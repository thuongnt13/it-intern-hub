using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITInternHub.Models;
using ITInternHub.Models.ViewModels;

namespace ITInternHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ROLE_ADMIN,ROLE_UNIVERSITY")] // Cả Admin và Nhà trường đều truy cập được
    [Route("Admin/InternshipTracking")]
    public class InternshipTrackingController : Controller
    {
        private readonly ITInternHubDbContext _context;

        public InternshipTrackingController(ITInternHubDbContext context)
        {
            _context = context;
        }

        // ==============================================================
        // 1. SỔ THEO DÕI THỰC TẬP SINH: /Admin/InternshipTracking
        // ==============================================================
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int? termId, string? keyword)
        {
            ViewBag.Terms = await _context.Terms.OrderByDescending(t => t.Id).ToListAsync();
            ViewBag.CurrentTermId = termId;
            ViewBag.CurrentKeyword = keyword;

            var query = _context.Records
                .Include(r => r.Student).ThenInclude(s => s.User)
                .Include(r => r.Company)
                .Include(r => r.InternshipTerm)
                .Include(r => r.WeeklyReports)
                .AsQueryable();

            if (termId.HasValue && termId.Value > 0)
            {
                query = query.Where(r => r.InternshipTermId == termId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(r => r.Student.User.FullName.Contains(keyword)
                                      || r.Student.StudentCode.Contains(keyword)
                                      || r.Company.CompanyName.Contains(keyword));
            }

            var records = await query
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(records);
        }

        // ==============================================================
        // 2. THỐNG KÊ DOANH NGHIỆP TIẾP NHẬN: /Admin/InternshipTracking/CompanyStats
        // ==============================================================
        [HttpGet("CompanyStats")]
        public async Task<IActionResult> CompanyStats()
        {
            var stats = await _context.Records
                .Include(r => r.Company)
                .GroupBy(r => r.Company.CompanyName)
                .Select(g => new CompanyInternStatsViewModel
                {
                    CompanyName = g.Key,
                    StudentCount = g.Count()
                })
                .OrderByDescending(x => x.StudentCount)
                .ToListAsync();

            return View(stats);
        }

        // ==============================================================
        // 3. BẢNG ĐIỂM & ĐÁNH GIÁ CỦA DOANH NGHIỆP: /Admin/InternshipTracking/Gradebook
        // ==============================================================
        [HttpGet("Gradebook")]
        public async Task<IActionResult> Gradebook(int? termId, string? className)
        {
            ViewBag.Terms = await _context.Terms.OrderByDescending(t => t.Id).ToListAsync();
            ViewBag.CurrentTermId = termId;
            ViewBag.CurrentClassName = className;

            var query = _context.Records
                .Include(r => r.Student).ThenInclude(s => s.User)
                .Include(r => r.Company)
                .Include(r => r.InternshipTerm)
                .Include(r => r.Evaluations)
                .AsQueryable();

            if (termId.HasValue && termId.Value > 0)
            {
                query = query.Where(r => r.InternshipTermId == termId.Value);
            }

            if (!string.IsNullOrWhiteSpace(className))
            {
                query = query.Where(r => r.Student.ClassName.Contains(className));
            }

            var gradebook = await query
                .OrderBy(r => r.Student.ClassName)
                .ThenBy(r => r.Student.StudentCode)
                .ToListAsync();

            return View(gradebook);
        }
    }
}