using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ITInternHub.Core.Application.DTOs;
using ITInternHub.Core.Application.UseCases.Jobs;
using ITInternHub.Models;
using ITInternHub.Models.Entities;
using ITInternHub.Services;

namespace ITInternHub.Controllers
{
    [ApiController]
    [Route("api/v1/employer")]
    [Authorize(Roles = "ROLE_EMPLOYER")]
    public class EmployerApiController : ControllerBase
    {
        private readonly ITInternHubDbContext _context;
        private readonly CreateJobPostUseCase _createJobUseCase;
        private readonly IAuditService _auditService;

        public EmployerApiController(
            ITInternHubDbContext context,
            CreateJobPostUseCase createJobUseCase,
            IAuditService auditService)
        {
            _context = context;
            _createJobUseCase = createJobUseCase;
            _auditService = auditService;
        }

        // ==============================================================
        // API 1: ĐĂNG TIN TUYỂN DỤNG (POST /api/v1/employer/jobs)
        // ==============================================================
        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobPostApiRequest request)
        {
            long userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var company = await _context.CompanyProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (company == null)
            {
                return BadRequest(new { success = false, message = "Chưa tìm thấy hồ sơ doanh nghiệp." });
            }

            var dto = new CreateJobPostDto
            {
                Title = request.Title,
                IndustryId = request.IndustryId,
                WorkingMode = request.WorkingMode,
                WorkingAddress = request.WorkingAddress,
                City = request.City,
                SalaryMin = request.SalaryMin,
                SalaryMax = request.SalaryMax,
                IsUnpaid = request.IsUnpaid,
                Deadline = request.Deadline,
                Quantity = request.Quantity,
                Description = request.Description,
                Requirements = request.Requirements,
                Benefits = request.Benefits,
                SelectedSkills = request.SelectedSkills ?? new List<JobSkillItemDto>()
            };

            // newJobId trả về kiểu long
            var (success, message, newJobId) = await _createJobUseCase.ExecuteAsync(dto, company.Id, userId);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new
            {
                success = true,
                message = "Đăng tin tuyển dụng thành công, đang chờ Quản trị viên duyệt!",
                jobId = newJobId,
                title = request.Title
            });
        }

        // ==============================================================
        // API 2: ĐÁNH GIÁ THỰC TẬP SINH (POST /api/v1/employer/evaluations)
        // ==============================================================
        [HttpPost("evaluations")]
        public async Task<IActionResult> SubmitEvaluation([FromBody] SubmitEvaluationApiRequest request)
        {
            long userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var record = await _context.Records
                .Include(r => r.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(r => r.Id == request.RecordId && r.Company.UserId == userId);

            if (record == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy hồ sơ thực tập hoặc bạn không có quyền đánh giá sinh viên này." });
            }

            // Tính điểm tổng kết: Chuyên môn (50%) + Kỷ luật (30%) + Kỹ năng mềm (20%)
            decimal finalScore = Math.Round((request.TechnicalScore * 0.5m) + (request.DisciplineScore * 0.3m) + (request.SoftSkillScore * 0.2m), 1);

            var evaluation = new Evaluation
            {
                InternshipRecordId = record.Id,
                EvaluationRound = request.Round, // 1: Giữa kỳ, 2: Cuối kỳ
                DisciplineScore = request.DisciplineScore,
                TechnicalScore = request.TechnicalScore,
                SoftSkillScore = request.SoftSkillScore,
                FinalScore = finalScore,
                Comments = request.Comments?.Trim() ?? "Đạt yêu cầu",
                ImprovementPoints = request.ImprovementPoints?.Trim(),
                HiringRecommendation = request.HiringRecommendation,
                EvaluatedBy = User.Identity?.Name ?? "TechLead",
                EvaluatedAt = DateTime.UtcNow
            };

            _context.Evaluations.Add(evaluation);

            if (request.Round == 2)
            {
                record.Status = 2; // Hoàn thành
                record.ActualEndDate = DateOnly.FromDateTime(DateTime.Today);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "INTERN_EVALUATED", "internship.evaluations", evaluation.Id,
                $"Doanh nghiệp đánh giá sinh viên [{record.Student.User.FullName}] đạt điểm tổng kết: {finalScore}/10.");

            return Ok(new
            {
                success = true,
                message = "Gửi phiếu đánh giá thực tập thành công!",
                data = new
                {
                    evaluationId = evaluation.Id,
                    studentName = record.Student.User.FullName,
                    round = request.Round == 1 ? "Giữa kỳ" : "Cuối kỳ",
                    finalScore = finalScore,
                    hiringRecommendation = request.HiringRecommendation
                }
            });
        }

        private long GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idClaim, out var val) ? val : 0;
        }
    }

    public class CreateJobPostApiRequest
    {
        public string Title { get; set; } = string.Empty;
        public int IndustryId { get; set; } = 1;
        public byte WorkingMode { get; set; } = 1;
        public string WorkingAddress { get; set; } = "Công viên phần mềm Quang Trung";
        public string City { get; set; } = "TP.HCM";
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool IsUnpaid { get; set; } = false;
        public DateTime Deadline { get; set; } = DateTime.Today.AddMonths(1);
        public int Quantity { get; set; } = 3;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Benefits { get; set; } = string.Empty;
        public List<JobSkillItemDto> SelectedSkills { get; set; } = new();
    }

    public class SubmitEvaluationApiRequest
    {
        public long RecordId { get; set; }
        public byte Round { get; set; } = 2;
        public decimal DisciplineScore { get; set; } = 9.0m;
        public decimal TechnicalScore { get; set; } = 8.5m;
        public decimal SoftSkillScore { get; set; } = 8.0m;
        public string Comments { get; set; } = string.Empty;
        public string? ImprovementPoints { get; set; }
        public bool HiringRecommendation { get; set; } = true;
    }
}