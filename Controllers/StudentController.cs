using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ITInternHub.Models;
using ITInternHub.Models.Entities;

namespace ITInternHub.Controllers
{
    [Authorize(Roles = "ROLE_STUDENT")]
    [Route("Student")]
    public class StudentController : Controller
    {
        private readonly ITInternHubDbContext _context;

        public StudentController(ITInternHubDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. HỒ SƠ NĂNG LỰC & CV: /Student/Profile
        // ==========================================
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            long userId = GetCurrentUserId();
            var student = await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                // Khởi tạo nếu chưa có
                student = new StudentProfile
                {
                    UserId = userId,
                    StudentCode = "SV" + userId.ToString("D5"),
                    ClassName = "63K1-CNTT",
                    Major = "Kỹ thuật phần mềm",
                    AdmissionYear = DateTime.Now.Year,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.StudentProfiles.Add(student);
                await _context.SaveChangesAsync();
            }

            ViewBag.Skills = await _context.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            return View(student);
        }

        [HttpPost("Profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(StudentProfile model, List<int> selectedSkills)
        {
            long userId = GetCurrentUserId();
            var student = await _context.StudentProfiles
                .Include(s => s.StudentSkills)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return NotFound();

            student.StudentCode = model.StudentCode?.Trim() ?? student.StudentCode;
            student.ClassName = model.ClassName?.Trim() ?? student.ClassName;
            student.Major = model.Major?.Trim() ?? student.Major;
            student.Gpa = model.Gpa;
            student.GithubUrl = model.GithubUrl?.Trim();
            student.LinkedinUrl = model.LinkedinUrl?.Trim();
            student.Bio = model.Bio;
            student.UpdatedAt = DateTime.UtcNow;

            // Cập nhật bộ thẻ kỹ năng
            _context.StudentSkills.RemoveRange(student.StudentSkills);
            if (selectedSkills != null && selectedSkills.Any())
            {
                foreach (var sId in selectedSkills)
                {
                    _context.StudentSkills.Add(new StudentSkill
                    {
                        StudentId = student.Id,
                        SkillId = sId,
                        ProficiencyLevel = 2
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hồ sơ năng lực thành công!";
            return RedirectToAction(nameof(Profile));
        }

        // ==========================================
        // 2. ĐƠN ỨNG TUYỂN CỦA TÔI: /Student/Applications
        // ==========================================
        [HttpGet("Applications")]
        public async Task<IActionResult> Applications()
        {
            long userId = GetCurrentUserId();
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return RedirectToAction(nameof(Profile));

            var myApps = await _context.Applications
                .Include(a => a.JobPost).ThenInclude(j => j.Company)
                .Include(a => a.Interviews)
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(myApps);
        }

        // Action Nộp đơn ứng tuyển
        [HttpPost("Apply")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(long jobPostId, string cvUrl, string? coverLetter)
        {
            long userId = GetCurrentUserId();
            var student = await _context.StudentProfiles
                .Include(s => s.StudentSkills)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return Forbid();

            // Kiểm tra nộp trùng lặp
            if (await _context.Applications.AnyAsync(a => a.JobPostId == jobPostId && a.StudentId == student.Id))
            {
                TempData["ErrorMessage"] = "Bạn đã nộp hồ sơ cho bài đăng này rồi!";
                return RedirectToAction("JobDetail", "Home", new { id = jobPostId });
            }

            // Tính % Match Score tại thời điểm nộp
            var job = await _context.JobPosts.Include(j => j.JobSkills).FirstOrDefaultAsync(j => j.Id == jobPostId);
            double score = 0;
            if (job != null)
            {
                var studentSkillIds = student.StudentSkills.Select(ss => ss.SkillId).ToList();
                var req = job.JobSkills.Where(js => js.PriorityLevel == 1).Select(js => js.SkillId).ToList();
                var pref = job.JobSkills.Where(js => js.PriorityLevel == 2).Select(js => js.SkillId).ToList();

                int matchedReq = req.Count(id => studentSkillIds.Contains(id));
                int matchedPref = pref.Count(id => studentSkillIds.Contains(id));

                double reqScore = req.Count > 0 ? ((double)matchedReq / req.Count) * 70.0 : 70.0;
                double prefScore = pref.Count > 0 ? ((double)matchedPref / pref.Count) * 30.0 : 30.0;
                score = Math.Round(reqScore + prefScore, 1);
            }

            var application = new Models.Entities.Application
            {
                JobPostId = jobPostId,
                StudentId = student.Id,
                CvUrl = cvUrl.Trim(),
                CoverLetter = coverLetter?.Trim(),
                MatchScore = (decimal)score,
                Status = 1, // 1: APPLIED
                AppliedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);
            if (job != null) job.ApplicationCount += 1;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Nộp hồ sơ ứng tuyển thành công! Nhà tuyển dụng sẽ xem xét hồ sơ của bạn.";
            return RedirectToAction(nameof(Applications));
        }

        // ==========================================
        // 3. NHẬT KÝ THỰC TẬP TUẦN: /Student/WeeklyLogs
        // ==========================================
        [HttpGet("WeeklyLogs")]
        public async Task<IActionResult> WeeklyLogs()
        {
            long userId = GetCurrentUserId();
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return RedirectToAction(nameof(Profile));

            // Kiểm tra xem sinh viên đã có hồ sơ trúng tuyển hoặc tiếp nhận thực tập chưa
            var acceptedRecord = await _context.Records
                .Include(r => r.Company)
                .Include(r => r.WeeklyReports)
                .FirstOrDefaultAsync(r => r.StudentId == student.Id);

            ViewBag.HasAcceptedRecord = acceptedRecord != null;
            ViewBag.InternshipRecord = acceptedRecord;

            var reports = acceptedRecord != null
                ? acceptedRecord.WeeklyReports.OrderByDescending(w => w.WeekNumber).ToList()
                : new List<WeeklyReport>();

            return View(reports);
        }

        [HttpPost("WeeklyLogs/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWeeklyLog(int weekNumber, string workSummary)
        {
            long userId = GetCurrentUserId();
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return Forbid();

            var acceptedRecord = await _context.Records.FirstOrDefaultAsync(r => r.StudentId == student.Id);
            if (acceptedRecord == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa có giao kết thực tập chính thức với doanh nghiệp nào.";
                return RedirectToAction(nameof(WeeklyLogs));
            }

            if (string.IsNullOrWhiteSpace(workSummary))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung công việc đã làm trong tuần.";
                return RedirectToAction(nameof(WeeklyLogs));
            }

            var report = new WeeklyReport
            {
                InternshipRecordId = acceptedRecord.Id,
                WeekNumber = weekNumber,
                WorkSummary = workSummary.Trim(),
                Status = 1, // 1: Đã nộp, chờ mentor nhận xét
                SubmittedAt = DateTime.UtcNow
            };

            _context.WeeklyReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã nộp nhật ký thực tập tuần {weekNumber} thành công!";
            return RedirectToAction(nameof(WeeklyLogs));
        }

        private long GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idClaim, out var val) ? val : 0;
        }

        // SINH VIÊN CHẤP NHẬN LỜI MỜI THỰC TẬP (OFFER ACCEPTED)
        [HttpPost("Applications/AcceptOffer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOffer(long applicationId)
        {
            long userId = GetCurrentUserId();
            var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return Forbid();

            var app = await _context.Applications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentId == student.Id);

            if (app == null || app.Status != 5) // Chỉ chấp nhận khi đang ở trạng thái 5: Gửi Offer
            {
                TempData["ErrorMessage"] = "Không thể chấp nhận lời mời thực tập này.";
                return RedirectToAction(nameof(Applications));
            }

            app.Status = 6; // 6: Tiếp nhận chính thức
            app.UpdatedAt = DateTime.UtcNow;

            // Đảm bảo có kỳ thực tập
            var term = await _context.Terms.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
            if (term == null)
            {
                term = new Term
                {
                    Name = "Học kỳ Doanh nghiệp 2026",
                    AcademicYear = "2026-2027",
                    Semester = 1,
                    StartDate = DateOnly.FromDateTime(DateTime.Today),
                    EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(4)),
                    IsActive = true
                };
                _context.Terms.Add(term);
                await _context.SaveChangesAsync();
            }

            // Tạo sổ thực tập
            if (!await _context.Records.AnyAsync(r => r.StudentId == student.Id && r.CompanyId == app.JobPost.CompanyId))
            {
                _context.Records.Add(new Record
                {
                    StudentId = student.Id,
                    CompanyId = app.JobPost.CompanyId,
                    InternshipTermId = term.Id,
                    ApplicationId = app.Id,
                    JobTitle = app.JobPost.Title,
                    SupervisorName = "Người hướng dẫn doanh nghiệp",
                    SupervisorEmail = "mentor@company.com",
                    StartDate = DateOnly.FromDateTime(DateTime.Today),
                    ExpectedEndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
                    Status = 1
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Chúc mừng! Bạn đã chính thức tiếp nhận vị trí thực tập và mở khóa Sổ nhật ký tuần.";
            return RedirectToAction(nameof(Applications));
        }
    }
}