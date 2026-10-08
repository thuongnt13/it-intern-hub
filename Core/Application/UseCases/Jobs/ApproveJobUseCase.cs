using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Jobs
{
    // Ca sử dụng độc lập (Single Responsibility)
    public class ApproveJobUseCase
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly IAuditService _auditService;

        public ApproveJobUseCase(IJobPostRepository jobRepository, IAuditService auditService)
        {
            _jobRepository = jobRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(long jobId, long adminUserId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return (false, "Không tìm thấy bài đăng tuyển dụng.");
            }

            try
            {
                // 1. GỌI HÀNH VI CỦA THỰC THỂ MIỀN (Domain logic)
                job.Approve();

                // 2. LƯU QUA REPOSITORY
                await _jobRepository.UpdateAsync(job);

                // 3. GHI AUDIT LOG
                await _auditService.LogAsync(
                    userId: adminUserId,
                    action: "JOB_APPROVED",
                    tableName: "company.job_posts",
                    recordId: job.Id,
                    description: $"Admin đã phê duyệt bài đăng tuyển dụng: '{job.Title}'"
                );

                return (true, $"Phê duyệt tin '{job.Title}' thành công!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}