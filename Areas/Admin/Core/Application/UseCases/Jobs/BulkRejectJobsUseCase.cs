using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Jobs
{
    public class BulkRejectJobsUseCase
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly IAuditService _auditService;

        public BulkRejectJobsUseCase(IJobPostRepository jobRepository, IAuditService auditService)
        {
            _jobRepository = jobRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message, int Count)> ExecuteAsync(List<long> jobIds, string reason, long adminUserId)
        {
            if (jobIds == null || !jobIds.Any())
            {
                return (false, "Vui lòng chọn ít nhất một bài đăng để từ chối.", 0);
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Bắt buộc phải nhập lý do từ chối.", 0);
            }

            var jobs = await _jobRepository.GetByIdsAsync(jobIds);
            int rejectedCount = 0;

            foreach (var job in jobs)
            {
                if (job.Status == 1)
                {
                    job.Reject(reason);
                    rejectedCount++;
                }
            }

            if (rejectedCount > 0)
            {
                await _jobRepository.UpdateRangeAsync(jobs);
                await _auditService.LogAsync(adminUserId, "BULK_JOBS_REJECTED", "company.job_posts", null, $"Admin từ chối hàng loạt {rejectedCount} bài đăng. Lý do: {reason}");
                return (true, $"Đã từ chối {rejectedCount} bài đăng.", rejectedCount);
            }

            return (false, "Không có bài đăng nào hợp lệ để từ chối.", 0);
        }
    }
}