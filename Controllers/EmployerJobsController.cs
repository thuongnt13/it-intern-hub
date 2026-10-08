using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Core.Application.DTOs;
using ITInternHub.Core.Application.UseCases.Jobs;
using ITInternHub.Models;
using ITInternHub.Models.Entities;

namespace ITInternHub.Controllers
{
    [Authorize(Roles = "ROLE_EMPLOYER")]
    [Route("Employer/Jobs")]
    public class EmployerJobsController : Controller
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly CreateJobPostUseCase _createJobUseCase;
        private readonly UpdateJobPostUseCase _updateJobUseCase;
        private readonly ITInternHubDbContext _context;

        public EmployerJobsController(
            IJobPostRepository jobRepository,
            CreateJobPostUseCase createJobUseCase,
            UpdateJobPostUseCase updateJobUseCase,
            ITInternHubDbContext context)
        {
            _jobRepository = jobRepository;
            _createJobUseCase = createJobUseCase;
            _updateJobUseCase = updateJobUseCase;
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH TIN: /Employer/Jobs hoặc /Employer/Jobs/Index
        // ==========================================
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var jobs = await _jobRepository.GetJobsByCompanyUserIdAsync(userId);
            return View(jobs);
        }

        // ==========================================
        // 2. MÀN HÌNH TẠO TIN: /Employer/Jobs/Create (GET)
        // ==========================================
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Industries = await _context.Industries.Where(i => i.IsActive).ToListAsync();
            ViewBag.Skills = await _context.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

            return View(new CreateJobPostDto());
        }

        // ==========================================
        // 3. XỬ LÝ LƯU TIN MỚI: /Employer/Jobs/Create (POST)
        // ==========================================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateJobPostDto dto, string selectedSkillsData)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId)) return Forbid();

            // Lấy hoặc tự động tạo hồ sơ công ty
            var companyProfile = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (companyProfile == null)
            {
                companyProfile = new CompanyProfile
                {
                    UserId = userId,
                    CompanyName = User.Identity?.Name ?? "Doanh nghiệp",
                    TaxCode = "MST" + userId.ToString("D6"),
                    Address = dto.WorkingAddress ?? "Chưa cập nhật",
                    City = dto.City ?? "Hà Nội",
                    CompanyEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "hr@company.com",
                    CreatedAt = DateTime.UtcNow
                };
                _context.CompanyProfiles.Add(companyProfile);
                await _context.SaveChangesAsync();
            }

            // Giải mã danh sách thẻ kỹ năng gửi lên từ View
            if (!string.IsNullOrEmpty(selectedSkillsData))
            {
                dto.SelectedSkills = selectedSkillsData.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(item =>
                    {
                        var parts = item.Split(':');
                        return new JobSkillItemDto
                        {
                            SkillId = int.Parse(parts[0]),
                            PriorityLevel = byte.Parse(parts[1])
                        };
                    }).ToList();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Industries = await _context.Industries.Where(i => i.IsActive).ToListAsync();
                ViewBag.Skills = await _context.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
                return View(dto);
            }

            var (success, message, _) = await _createJobUseCase.ExecuteAsync(dto, companyProfile.Id, userId);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Industries = await _context.Industries.Where(i => i.IsActive).ToListAsync();
                ViewBag.Skills = await _context.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
                return View(dto);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. MÀN HÌNH SỬA TIN: /Employer/Jobs/Edit/{id} (GET)
        // ==========================================
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId)) return Forbid();

            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null) return NotFound();

            if (job.Company.UserId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa bài đăng này.";
                return RedirectToAction(nameof(Index));
            }

            // Ràng buộc: Đã duyệt thì không cho sửa
            if (job.Status == 2)
            {
                TempData["ErrorMessage"] = "Tin tuyển dụng đã được duyệt công khai, không thể chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Industries = await _context.Industries.Where(i => i.IsActive).ToListAsync();
            ViewBag.Skills = await _context.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

            var dto = new EditJobPostDto
            {
                Id = job.Id,
                Title = job.Title,
                IndustryId = job.IndustryId,
                WorkingMode = job.WorkingMode,
                WorkingAddress = job.WorkingAddress,
                City = job.City,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                IsUnpaid = job.IsUnpaid,
                Deadline = job.Deadline.ToDateTime(TimeOnly.MinValue),
                Quantity = job.Quantity,
                Description = job.Description,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                Status = job.Status,
                RejectionReason = job.RejectionReason,
                SelectedSkills = job.JobSkills.Select(js => new JobSkillItemDto
                {
                    SkillId = js.SkillId,
                    PriorityLevel = js.PriorityLevel
                }).ToList()
            };

            return View(dto);
        }

        // ==========================================
        // 5. XỬ LÝ LƯU SỬA TIN: /Employer/Jobs/Edit/{id} (POST)
        // ==========================================
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, EditJobPostDto dto, string selectedSkillsData)
        {
            if (id != dto.Id) return BadRequest();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId)) return Forbid();

            var companyProfile = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (companyProfile == null) return Forbid();

            if (!string.IsNullOrEmpty(selectedSkillsData))
            {
                dto.SelectedSkills = selectedSkillsData.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(item =>
                    {
                        var parts = item.Split(':');
                        return new JobSkillItemDto
                        {
                            SkillId = int.Parse(parts[0]),
                            PriorityLevel = byte.Parse(parts[1])
                        };
                    }).ToList();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Industries = await _context.Industries.Where(i => i.IsActive).ToListAsync();
                ViewBag.Skills = await _context.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
                return View(dto);
            }

            var (success, message) = await _updateJobUseCase.ExecuteAsync(dto, companyProfile.Id, userId);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Industries = await _context.Industries.Where(i => i.IsActive).ToListAsync();
                ViewBag.Skills = await _context.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
                return View(dto);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}