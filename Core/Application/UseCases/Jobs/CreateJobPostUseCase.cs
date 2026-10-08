using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Core.Application.DTOs;
using ITInternHub.Core.Domain.Enums;
using ITInternHub.Models.Entities;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Jobs
{
    public class CreateJobPostUseCase
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly IAuditService _auditService;

        public CreateJobPostUseCase(IJobPostRepository jobRepository, IAuditService auditService)
        {
            _jobRepository = jobRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message, long JobId)> ExecuteAsync(CreateJobPostDto dto, long companyProfileId, long userId)
        {
            if (dto.Deadline <= DateTime.Today)
            {
                return (false, "Hạn nộp hồ sơ phải lớn hơn ngày hiện tại.", 0);
            }

            if (!dto.SelectedSkills.Any(s => s.PriorityLevel == 1))
            {
                return (false, "Bài đăng tuyển dụng bắt buộc phải có ít nhất 1 Kỹ năng tiên quyết (Required).", 0);
            }

            try
            {
                // 1. Khởi tạo Entity JobPost
                var jobPost = new JobPost
                {
                    CompanyId = companyProfileId,
                    IndustryId = dto.IndustryId,
                    Title = dto.Title.Trim(),
                    Slug = GenerateSlug(dto.Title),
                    Quantity = dto.Quantity,
                    WorkingMode = dto.WorkingMode,
                    WorkingAddress = dto.WorkingAddress.Trim(),
                    City = dto.City.Trim(),
                    SalaryMin = dto.IsUnpaid ? null : dto.SalaryMin,
                    SalaryMax = dto.IsUnpaid ? null : dto.SalaryMax,
                    IsUnpaid = dto.IsUnpaid,

                    // SỬA LỖI CS0029 Ở ĐÂY: Chuyển DateTime sang DateOnly của .NET 8
                    Deadline = DateOnly.FromDateTime(dto.Deadline),

                    Description = dto.Description,
                    Requirements = dto.Requirements,
                    Benefits = dto.Benefits,
                    Status = (byte)JobStatus.PendingApproval, // 1: Chờ Admin duyệt
                    CreatedAt = DateTime.UtcNow
                };

                // 2. Chuyển đổi danh sách kỹ năng
                var jobSkills = dto.SelectedSkills.Select(s => new JobSkill
                {
                    SkillId = s.SkillId,
                    PriorityLevel = s.PriorityLevel
                }).ToList();

                // 3. Lưu xuống cơ sở dữ liệu qua Repository
                long newJobId = await _jobRepository.CreateJobWithSkillsAsync(jobPost, jobSkills);

                // 4. Ghi Audit Log
                await _auditService.LogAsync(
                    userId: userId,
                    action: "JOB_CREATED",
                    tableName: "company.job_posts",
                    recordId: newJobId,
                    description: $"Doanh nghiệp đã đăng tin tuyển dụng mới: '{jobPost.Title}'"
                );

                return (true, "Đăng tin tuyển dụng thành công! Vui lòng chờ Ban quản trị phê duyệt.", newJobId);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", 0);
            }
        }

        private static string GenerateSlug(string title)
        {
            return title.ToLower()
                .Replace(" ", "-")
                .Replace("/", "-") + "-" + DateTime.UtcNow.Ticks.ToString()[^6..];
        }
    }
}