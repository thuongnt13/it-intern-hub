using ITInternHub.Models.Entities;

namespace ITInternHub.Core.Application.Contracts.Repositories
{
    public interface IJobPostRepository
    {
        Task<JobPost?> GetByIdAsync(long id);

        // 2 HÀM DÀNH CHO DUYỆT HÀNG LOẠT
        Task<List<JobPost>> GetByIdsAsync(List<long> ids);
        Task UpdateRangeAsync(List<JobPost> jobPosts);
        Task<List<JobPost>> GetPublishedJobsAsync();
        Task<List<JobPost>> GetPendingJobsAsync();
        Task<List<JobPost>> GetJobsByCompanyUserIdAsync(long userId);
        Task<long> CreateJobWithSkillsAsync(JobPost jobPost, List<JobSkill> skills);
        Task UpdateJobWithSkillsAsync(JobPost jobPost, List<JobSkill> newSkills);
        Task UpdateAsync(JobPost jobPost);
    }
}