using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Core.Application.DTOs;
using ITInternHub.Models.Entities;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Jobs
{
    public class UpdateJobPostUseCase
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly IAuditService _auditService;

        public UpdateJobPostUseCase(IJobPostRepository jobRepository, IAuditService auditService)
        {
            _jobRepository = jobRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(EditJobPostDto dto, long companyProfileId, long userId)
        {
            var job = await _jobRepository.GetByIdAsync(dto.Id);
            if (job == null)
            {
                return (false, "Không tìm thấy bài đăng tuyển dụng.");
            }

            // Kiểm tra quyền sở hữu
            if (job.CompanyId != companyProfileId)
            {
                return (false, "Bạn không có quyền chỉnh sửa bài đăng này.");
            }

            if (dto.Deadline <= DateTime.Today)
            {
                return (false, "Hạn nộp hồ sơ phải lớn hơn ngày hiện tại.");
            }

            if (!dto.SelectedSkills.Any(s => s.PriorityLevel == 1))
            {
                return (false, "Bài đăng phải có ít nhất 1 kỹ năng tiên quyết (Required).");
            }

            try
            {
                // GỌI HÀNH VI CỦA THỰC THỂ MIỀN (Domain Model kiểm tra trạng thái và cập nhật)
                job.UpdateDetails(
                    title: dto.Title,
                    industryId: dto.IndustryId,
                    workingMode: dto.WorkingMode,
                    workingAddress: dto.WorkingAddress,
                    city: dto.City,
                    salaryMin: dto.SalaryMin,
                    salaryMax: dto.SalaryMax,
                    isUnpaid: dto.IsUnpaid,
                    deadline: DateOnly.FromDateTime(dto.Deadline),
                    quantity: dto.Quantity,
                    description: dto.Description,
                    requirements: dto.Requirements,
                    benefits: dto.Benefits
                );

                // Chuyển đổi danh sách kỹ năng mới
                var newSkills = dto.SelectedSkills.Select(s => new JobSkill
                {
                    JobPostId = job.Id,
                    SkillId = s.SkillId,
                    PriorityLevel = s.PriorityLevel
                }).ToList();

                // Lưu thay đổi trên cùng một bản ghi qua Repository
                await _jobRepository.UpdateJobWithSkillsAsync(job, newSkills);

                // Ghi vết kiểm toán
                await _auditService.LogAsync(
                    userId: userId,
                    action: "JOB_UPDATED",
                    tableName: "company.job_posts",
                    recordId: job.Id,
                    description: $"Doanh nghiệp đã cập nhật bài đăng '{job.Title}'. Trạng thái: Chờ duyệt."
                );

                return (true, "Cập nhật bài đăng tuyển dụng thành công!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}