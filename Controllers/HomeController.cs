using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITInternHub.Models;
using ITInternHub.Models.ViewModels;

namespace ITInternHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITInternHubDbContext _context;

        public HomeController(ILogger<HomeController> logger, ITInternHubDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ==========================================
        // 1. TRANG CHỦ: TÌM KIẾM & XEM DANH SÁCH VIỆC LÀM (TOPCV STYLE)
        // ==========================================
        // GET: / (Trang chủ phân trang)
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, int? industryId, string? city, string? skill, int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 9; // 9 bài mỗi trang

            ViewBag.Industries = await _context.Industries.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder).ToListAsync();
            ViewBag.PopularSkills = await _context.Skills.Where(s => s.IsActive && s.IsHot).Take(8).ToListAsync();

            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentIndustryId = industryId;
            ViewBag.CurrentCity = city;
            ViewBag.CurrentSkill = skill;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var query = _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.Industry)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.Status == 2 && j.Deadline >= today)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) || j.Company.CompanyName.Contains(keyword));
            }
            if (industryId.HasValue && industryId.Value > 0)
            {
                query = query.Where(j => j.IndustryId == industryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(j => j.City == city);
            }
            if (!string.IsNullOrWhiteSpace(skill))
            {
                query = query.Where(j => j.JobSkills.Any(js => js.Skill.Name == skill));
            }

            var allFilteredJobs = await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
            int totalCount = allFilteredJobs.Count;

            // Tính Match Score cho Sinh viên
            List<int> studentSkillIds = new();
            bool isStudent = User.Identity?.IsAuthenticated == true && User.IsInRole("ROLE_STUDENT");
            if (isStudent)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var userId))
                {
                    var studentProfile = await _context.StudentProfiles
                        .Include(s => s.StudentSkills)
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (studentProfile != null)
                    {
                        studentSkillIds = studentProfile.StudentSkills.Select(ss => ss.SkillId).ToList();
                    }
                }
            }

            var jobCards = allFilteredJobs.Select(job =>
            {
                double score = 0;
                if (studentSkillIds.Any())
                {
                    var reqSkills = job.JobSkills.Where(js => js.PriorityLevel == 1).Select(js => js.SkillId).ToList();
                    var prefSkills = job.JobSkills.Where(js => js.PriorityLevel == 2).Select(js => js.SkillId).ToList();

                    int matchedReq = reqSkills.Count(id => studentSkillIds.Contains(id));
                    int matchedPref = prefSkills.Count(id => studentSkillIds.Contains(id));

                    double reqScore = reqSkills.Count > 0 ? ((double)matchedReq / reqSkills.Count) * 70.0 : 70.0;
                    double prefScore = prefSkills.Count > 0 ? ((double)matchedPref / prefSkills.Count) * 30.0 : 30.0;
                    score = Math.Round(reqScore + prefScore, 1);
                }

                return new JobCardViewModel
                {
                    Id = job.Id,
                    Title = job.Title,
                    CompanyName = job.Company?.CompanyName ?? "Doanh nghiệp đối tác",
                    CompanyLogo = job.Company?.LogoUrl ?? "https://ui-avatars.com/api/?name=Company&background=103057&color=fff",
                    City = job.City,
                    SalaryMin = job.SalaryMin,
                    SalaryMax = job.SalaryMax,
                    IsUnpaid = job.IsUnpaid,
                    Deadline = job.Deadline.ToDateTime(TimeOnly.MinValue),
                    RequiredSkills = job.JobSkills.Select(js => js.Skill.Name).ToList(),
                    MatchScore = score
                };
            }).ToList();

            if (studentSkillIds.Any())
            {
                jobCards = jobCards.OrderByDescending(j => j.MatchScore).ToList();
            }

            // ÁP DỤNG SKIP / TAKE PHÂN TRANG
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var pagedJobCards = jobCards.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new HomeIndexViewModel
            {
                FeaturedJobs = pagedJobCards,
                TotalJobsCount = totalCount,
                IsStudentLoggedIn = isStudent,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // ==========================================
        // 2. XEM CHI TIẾT CÔNG VIỆC: /Jobs/Detail/{id}
        // ==========================================
        [HttpGet("Jobs/Detail/{id}")]
        public async Task<IActionResult> JobDetail(long id)
        {
            var job = await _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.Industry)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == id && j.Status == 2);

            if (job == null) return NotFound();

            // Tăng lượt xem tin
            job.ViewCount += 1;
            await _context.SaveChangesAsync();

            // Kiểm tra xem Sinh viên hiện tại đã nộp đơn cho vị trí này chưa
            ViewBag.HasApplied = false;
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("ROLE_STUDENT"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var userId))
                {
                    var studentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (studentProfile != null)
                    {
                        ViewBag.HasApplied = await _context.Applications
                            .AnyAsync(a => a.JobPostId == id && a.StudentId == studentProfile.Id);
                    }
                }
            }

            return View(job);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("api/jobs")]
        public async Task<IActionResult> GetJobsJson(string? keyword, int? industryId, string? city, string? skill, int page = 1)
        {
            int pageSize = 10;
            var today = DateOnly.FromDateTime(DateTime.Today);

            var query = _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.Status == 2 && j.Deadline >= today)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(j => j.Title.Contains(keyword) || j.Company.CompanyName.Contains(keyword));

            if (industryId.HasValue && industryId.Value > 0)
                query = query.Where(j => j.IndustryId == industryId.Value);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(j => j.City == city);

            if (!string.IsNullOrWhiteSpace(skill))
                query = query.Where(j => j.JobSkills.Any(js => js.Skill.Name == skill));

            int totalCount = await query.CountAsync();

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    id = j.Id,
                    title = j.Title,
                    company_name = j.Company.CompanyName,
                    city = j.City,
                    salary_min = j.SalaryMin,
                    salary_max = j.SalaryMax,
                    deadline = j.Deadline.ToString("yyyy-MM-dd"),
                    required_skills = j.JobSkills.Where(js => js.PriorityLevel == 1).Select(js => js.Skill.Name).ToList(),
                    preferred_skills = j.JobSkills.Where(js => js.PriorityLevel == 2).Select(js => js.Skill.Name).ToList()
                })
                .ToListAsync();

            // Dùng Json(...) để ép trả về chuỗi JSON thay vì render HTML
            return Json(new
            {
                page = page,
                page_size = pageSize,
                total_items = totalCount,
                total_pages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = jobs
            });
        }
    }
}