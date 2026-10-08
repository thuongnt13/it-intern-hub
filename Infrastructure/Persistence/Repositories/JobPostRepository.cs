using Microsoft.EntityFrameworkCore;
using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Models;
using ITInternHub.Models.Entities;

namespace ITInternHub.Infrastructure.Persistence.Repositories
{
    public class JobPostRepository : IJobPostRepository
    {
        private readonly ITInternHubDbContext _context;

        public JobPostRepository(ITInternHubDbContext context)
        {
            _context = context;
        }

        public async Task<JobPost?> GetByIdAsync(long id)
        {
            return await _context.JobPosts
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        // 1. CÀI ĐẶT LẤY NHIỀU BÀI ĐĂNG THEO DANH SÁCH IDS
        public async Task<List<JobPost>> GetByIdsAsync(List<long> ids)
        {
            return await _context.JobPosts
                .Where(j => ids.Contains(j.Id))
                .ToListAsync();
        }
        // BỔ SUNG: LẤY DANH SÁCH TẤT CẢ TIN ĐÃ ĐƯỢC PHÊ DUYỆT CÔNG KHAI
        public async Task<List<JobPost>> GetPublishedJobsAsync()
        {
            return await _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.Industry)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.Status == 2) // 2: Published
                .OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<JobPost>> GetPendingJobsAsync()
        {
            return await _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.Industry)
                .Where(j => j.Status == 1) // 1: PendingApproval
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<JobPost>> GetJobsByCompanyUserIdAsync(long userId)
        {
            return await _context.JobPosts
                .Include(j => j.Industry)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.Company.UserId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<long> CreateJobWithSkillsAsync(JobPost jobPost, List<JobSkill> skills)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.JobPosts.Add(jobPost);
                await _context.SaveChangesAsync();

                foreach (var sk in skills)
                {
                    sk.JobPostId = jobPost.Id;
                    _context.JobSkills.Add(sk);
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return jobPost.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateJobWithSkillsAsync(JobPost jobPost, List<JobSkill> newSkills)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.JobPosts.Update(jobPost);

                var oldSkills = await _context.JobSkills.Where(js => js.JobPostId == jobPost.Id).ToListAsync();
                _context.JobSkills.RemoveRange(oldSkills);

                _context.JobSkills.AddRange(newSkills);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(JobPost jobPost)
        {
            _context.JobPosts.Update(jobPost);
            await _context.SaveChangesAsync();
        }

        // 2. CÀI ĐẶT CẬP NHẬT NHIỀU BÀI ĐĂNG CÙNG LÚC
        public async Task UpdateRangeAsync(List<JobPost> jobPosts)
        {
            _context.JobPosts.UpdateRange(jobPosts);
            await _context.SaveChangesAsync();
        }
    }
}