using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Jobs
{
    public class BulkApproveJobsUseCase
    {
        private readonly IJobPostRepository _jobRepository;
        private readonly IAuditService _auditService;

        public BulkApproveJobsUseCase(IJobPostRepository jobRepository, IAuditService auditService)
        {
            _jobRepository = jobRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message, int Count)> ExecuteAsync(List<long> jobIds, long adminUserId)
        {
            if (jobIds == null || !jobIds.Any())
            {
                return (false, "Vui lòng chọn ít nhất một bài đăng để phê duyệt.", 0);
            }

            var jobs = await _jobRepository.GetByIdsAsync(jobIds);
            int approvedCount = 0;

            foreach (var job in jobs)
            {
                if (job.Status == 1) // 1: PendingApproval
                {
                    job.Approve();
                    approvedCount++;
                }
            }

            if (approvedCount > 0)
            {
                await _jobRepository.UpdateRangeAsync(jobs);
                await _auditService.LogAsync(adminUserId, "BULK_JOBS_APPROVED", "company.job_posts", null, $"Admin phê duyệt hàng loạt {approvedCount} bài đăng.");
                return (true, $"Đã phê duyệt thành công {approvedCount} bài đăng!", approvedCount);
            }

            return (false, "Không có bài đăng nào hợp lệ để duyệt.", 0);
        }
    }
}