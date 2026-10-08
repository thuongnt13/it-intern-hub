using ITInternHub.Models.Entities;

namespace ITInternHub.Core.Application.Contracts.Repositories
{
    public interface ISkillRepository
    {
        Task<Skill?> GetByIdAsync(int id);
        Task<List<Skill>> GetAllSkillsAsync(int? industryId = null, string? keyword = null);
        Task<List<Industry>> GetAllIndustriesAsync();
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<bool> IsSkillInUseAsync(int skillId); // Kiểm tra ràng buộc trước khi xóa
        Task AddAsync(Skill skill);
        Task UpdateAsync(Skill skill);
        Task DeleteAsync(Skill skill);
    }
}