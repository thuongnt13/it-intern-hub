using ITInternHub.Core.Application.Contracts.Repositories;

namespace ITInternHub.Core.Application.UseCases.Skills
{
    public class ToggleSkillHotUseCase
    {
        private readonly ISkillRepository _skillRepository;

        public ToggleSkillHotUseCase(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null) return (false, "Không tìm thấy kỹ năng.");

            skill.ToggleHot();
            await _skillRepository.UpdateAsync(skill);

            return (true, $"Đã cập nhật trạng thái HOT cho '{skill.Name}'.");
        }
    }
}