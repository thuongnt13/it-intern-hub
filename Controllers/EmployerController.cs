using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ITInternHub.Models;
using ITInternHub.Models.Entities;
using ITInternHub.Services;

namespace ITInternHub.Controllers
{
    [Authorize(Roles = "ROLE_EMPLOYER")]
    [Route("Employer")]
    public class EmployerController : Controller
    {
        private readonly ITInternHubDbContext _context;
        private readonly IAuditService _auditService;

        public EmployerController(ITInternHubDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // ==============================================================
        // 1. SÀNG LỌC CV (% MATCH SCORE): /Employer/Applications
        // ==============================================================
        [HttpGet("Applications")]
        public async Task<IActionResult> Applications(long? jobPostId, byte? status, string? keyword)
        {
            long userId = GetCurrentUserId();
            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (company == null) return Forbid();

            // Lấy danh sách tin tuyển dụng của công ty để hiển thị bộ lọc
            ViewBag.CompanyJobs = await _context.JobPosts
                .Where(j => j.CompanyId == company.Id)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentJobPostId = jobPostId;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentKeyword = keyword;

            var query = _context.Applications
                .Include(a => a.JobPost)
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Student).ThenInclude(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
                .Include(a => a.Interviews)
                .Where(a => a.JobPost.CompanyId == company.Id)
                .AsQueryable();

            if (jobPostId.HasValue && jobPostId.Value > 0)
            {
                query = query.Where(a => a.JobPostId == jobPostId.Value);
            }

            if (status.HasValue && status.Value > 0)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(a => a.Student.User.FullName.Contains(keyword) || a.Student.StudentCode.Contains(keyword));
            }

            // Ưu tiên xếp hạng ứng viên theo % Match Score giảm dần, sau đó theo GPA
            var applications = await query
                .OrderByDescending(a => a.MatchScore)
                .ThenByDescending(a => a.Student.Gpa)
                .ToListAsync();

            return View(applications);
        }

        // AJAX: Chuyển trạng thái phễu tuyển dụng
        [HttpPost("Applications/ChangeStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long applicationId, byte newStatus, string? note)
        {
            try
            {
                long userId = GetCurrentUserId();
                var app = await _context.Applications
                    .Include(a => a.JobPost)
                    .Include(a => a.Student).ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost.Company.UserId == userId);

                if (app == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ ứng tuyển." });

                byte oldStatus = app.Status;
                app.Status = newStatus;
                app.RecruiterNote = note?.Trim();
                app.UpdatedAt = DateTime.UtcNow;

                // Ghi nhận lịch sử chuyển trạng thái
                _context.StatusLogs.Add(new StatusLog
                {
                    ApplicationId = app.Id,
                    PreviousStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedByUserId = userId,
                    Comment = note,
                    CreatedAt = DateTime.UtcNow
                });

                // Nếu chuyển sang trạng thái 6 (ACCEPTED: Tiếp nhận chính thức) -> Tự động khởi tạo Sổ thực tập
                if (newStatus == 6)
                {
                    // 1. Đảm bảo luôn có Kỳ thực tập (nếu chưa có thì tự tạo)
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

                    // 2. Tạo Sổ thực tập kết nối Sinh viên - Doanh nghiệp - Nhà trường
                    var existingRecord = await _context.Records
                        .FirstOrDefaultAsync(r => r.StudentId == app.StudentId && r.CompanyId == app.JobPost.CompanyId);

                    if (existingRecord == null)
                    {
                        _context.Records.Add(new Record
                        {
                            StudentId = app.StudentId,
                            CompanyId = app.JobPost.CompanyId,
                            InternshipTermId = term.Id,
                            ApplicationId = app.Id,
                            JobTitle = app.JobPost.Title,
                            SupervisorName = User.Identity?.Name ?? "Người hướng dẫn TMA",
                            SupervisorEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "mentor@company.com",
                            SupervisorPhone = "02839978000",
                            StartDate = DateOnly.FromDateTime(DateTime.Today),
                            ExpectedEndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
                            Status = 1 // 1: Đang thực tập
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Ghi Audit Log
                await _auditService.LogAsync(userId, "APPLICATION_STATUS_CHANGED", "application.applications", app.Id,
                    $"Doanh nghiệp đổi trạng thái ứng viên [{app.Student.User.FullName}] sang trạng thái mã {newStatus}.");

                return Json(new { success = true, message = "Cập nhật trạng thái ứng viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Lên lịch hẹn phỏng vấn
        [HttpPost("Applications/ScheduleInterview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleInterview(long applicationId, DateTime startTime, byte format, string locationOrLink, string? interviewerName, string? candidateNote)
        {
            try
            {
                long userId = GetCurrentUserId();
                var app = await _context.Applications
                    .Include(a => a.JobPost)
                    .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost.Company.UserId == userId);

                if (app == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ." });

                app.Status = 3; // 3: INTERVIEW_SCHEDULED
                app.UpdatedAt = DateTime.UtcNow;

                var interview = new Interview
                {
                    ApplicationId = app.Id,
                    StartTime = startTime,
                    Format = format, // 1: Online, 2: Trực tiếp
                    LocationOrLink = locationOrLink.Trim(),
                    InterviewerName = interviewerName?.Trim(),
                    CandidateNote = candidateNote?.Trim()
                };
                _context.Interviews.Add(interview);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Lên lịch hẹn phỏng vấn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==============================================================
        // 2. QUẢN LÝ LỊCH PHỎNG VẤN: /Employer/Interviews
        // ==============================================================
        [HttpGet("Interviews")]
        public async Task<IActionResult> Interviews()
        {
            long userId = GetCurrentUserId();
            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (company == null) return Forbid();

            var interviews = await _context.Interviews
                .Include(i => i.Application).ThenInclude(a => a.Student).ThenInclude(s => s.User)
                .Include(i => i.Application).ThenInclude(a => a.JobPost)
                .Where(i => i.Application.JobPost.CompanyId == company.Id)
                .OrderByDescending(i => i.StartTime)
                .ToListAsync();

            return View(interviews);
        }

        // AJAX: Cập nhật kết quả phỏng vấn
        [HttpPost("Interviews/UpdateResult")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInterviewResult(long interviewId, string result)
        {
            try
            {
                var iv = await _context.Interviews.FindAsync(interviewId);
                if (iv == null) return Json(new { success = false, message = "Không tìm thấy buổi phỏng vấn." });

                iv.InterviewResult = result.Trim();
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật kết quả phỏng vấn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==============================================================
        // 3. ĐÁNH GIÁ THỰC TẬP SINH: /Employer/Evaluations
        // ==============================================================
        [HttpGet("Evaluations")]
        public async Task<IActionResult> Evaluations()
        {
            long userId = GetCurrentUserId();
            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (company == null) return Forbid();

            // SỬA Ở ĐÂY: Đổi r.Term thành r.InternshipTerm
            var records = await _context.Records
                .Include(r => r.Student).ThenInclude(s => s.User)
                .Include(r => r.Evaluations)
                .Include(r => r.InternshipTerm) // <-- ĐỔI THÀNH InternshipTerm
                .Where(r => r.CompanyId == company.Id)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(records);
        }

        // AJAX: Gửi phiếu nhận xét và chấm điểm thực tập về Nhà trường
        [HttpPost("Evaluations/Submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEvaluation(
            long recordId, byte round, decimal disciplineScore, decimal technicalScore,
            decimal softSkillScore, string comments, string? improvementPoints, bool hiringRecommendation)
        {
            try
            {
                long userId = GetCurrentUserId();
                var record = await _context.Records.FirstOrDefaultAsync(r => r.Id == recordId && r.Company.UserId == userId);
                if (record == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ thực tập." });

                // Tính điểm tổng kết: Chuyên môn (50%) + Kỷ luật (30%) + Kỹ năng mềm (20%)
                decimal finalScore = Math.Round((technicalScore * 0.5m) + (disciplineScore * 0.3m) + (softSkillScore * 0.2m), 1);

                var evaluation = new Evaluation
                {
                    InternshipRecordId = record.Id,
                    EvaluationRound = round, // 1: Giữa kỳ, 2: Cuối kỳ
                    DisciplineScore = disciplineScore,
                    TechnicalScore = technicalScore,
                    SoftSkillScore = softSkillScore,
                    FinalScore = finalScore,
                    Comments = comments.Trim(),
                    ImprovementPoints = improvementPoints?.Trim(),
                    HiringRecommendation = hiringRecommendation,
                    EvaluatedBy = User.Identity?.Name ?? "Người hướng dẫn",
                    EvaluatedAt = DateTime.UtcNow
                };

                _context.Evaluations.Add(evaluation);

                // Nếu là đánh giá cuối kỳ -> Cập nhật trạng thái sổ thực tập sang Hoàn thành (2: Completed)
                if (round == 2)
                {
                    record.Status = 2;
                    record.ActualEndDate = DateOnly.FromDateTime(DateTime.Today);
                }

                await _context.SaveChangesAsync();

                // Ghi vết kiểm toán
                await _auditService.LogAsync(userId, "INTERN_EVALUATED", "internship.evaluations", evaluation.Id,
                    $"Doanh nghiệp hoàn tất đánh giá thực tập sinh (Điểm tổng kết: {finalScore}).");

                return Json(new { success = true, message = $"Gửi đánh giá thành công! Điểm tổng kết: {finalScore}/10" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private long GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idClaim, out var val) ? val : 0;
        }
    }
}