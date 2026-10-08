using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Jobs
{
    public class RejectJobUseCase
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly IAuditService _auditService;

        public RejectJobUseCase(IJobPostRepository jobRepository, IAuditService auditService)
        {
            _jobRepository = jobRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(long jobId, string reason, long adminUserId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return (false, "Không tìm thấy bài đăng tuyển dụng.");
            }

            try
            {
                // Gọi hành vi của Entity
                job.Reject(reason);

                await _jobRepository.UpdateAsync(job);

                await _auditService.LogAsync(
                    userId: adminUserId,
                    action: "JOB_REJECTED",
                    tableName: "company.job_posts",
                    recordId: job.Id,
                    description: $"Từ chối bài đăng '{job.Title}'. Lý do: {reason}"
                );

                return (true, $"Đã từ chối bài đăng '{job.Title}'.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}