using Microsoft.EntityFrameworkCore;
using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Models;
using ITInternHub.Models.Entities;

namespace ITInternHub.Infrastructure.Persistence.Repositories
{
    public class SkillRepository : ISkillRepository
    {
        private readonly ITInternHubDbContext _context;

        public SkillRepository(ITInternHubDbContext context)
        {
            _context = context;
        }

        public async Task<Skill?> GetByIdAsync(int id)
        {
            return await _context.Skills
                .Include(s => s.Industry)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Skill>> GetAllSkillsAsync(int? industryId = null, string? keyword = null)
        {
            var query = _context.Skills
                .Include(s => s.Industry)
                .AsQueryable();

            if (industryId.HasValue && industryId.Value > 0)
            {
                query = query.Where(s => s.IndustryId == industryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s => s.Name.Contains(keyword) || (s.Category != null && s.Category.Contains(keyword)));
            }

            return await query
                .OrderBy(s => s.Industry.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<List<Industry>> GetAllIndustriesAsync()
        {
            return await _context.Industries.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder).ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            return await _context.Skills.AnyAsync(s => s.Name.ToLower() == name.Trim().ToLower() && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        public async Task<bool> IsSkillInUseAsync(int skillId)
        {
            bool usedInJobs = await _context.JobSkills.AnyAsync(js => js.SkillId == skillId);
            bool usedByStudents = await _context.StudentSkills.AnyAsync(ss => ss.SkillId == skillId);
            return usedInJobs || usedByStudents;
        }

        public async Task AddAsync(Skill skill)
        {
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Skill skill)
        {
            _context.Skills.Update(skill);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Skill skill)
        {
            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
        }
    }
}