using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Core.Application.DTOs;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Skills
{
    public class UpdateSkillUseCase
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IAuditService _auditService;

        public UpdateSkillUseCase(ISkillRepository skillRepository, IAuditService auditService)
        {
            _skillRepository = skillRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(UpdateSkillDto dto, long adminUserId)
        {
            var skill = await _skillRepository.GetByIdAsync(dto.Id);
            if (skill == null)
            {
                return (false, "Không tìm thấy thẻ kỹ năng cần sửa.");
            }

            if (await _skillRepository.ExistsByNameAsync(dto.Name, dto.Id))
            {
                return (false, $"Tên kỹ năng '{dto.Name}' đã trùng với một kỹ năng khác.");
            }

            try
            {
                skill.UpdateDetails(dto.Name, dto.IndustryId, dto.Category, dto.IsHot);
                await _skillRepository.UpdateAsync(skill);

                await _auditService.LogAsync(
                    userId: adminUserId,
                    action: "SKILL_UPDATED",
                    tableName: "catalog.skills",
                    recordId: skill.Id,
                    description: $"Cập nhật thẻ kỹ năng: '{skill.Name}'"
                );

                return (true, "Cập nhật kỹ năng thành công!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}