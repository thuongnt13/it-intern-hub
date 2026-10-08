using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Core.Application.DTOs;
using ITInternHub.Models.Entities;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Skills
{
    public class CreateSkillUseCase
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IAuditService _auditService;

        public CreateSkillUseCase(ISkillRepository skillRepository, IAuditService auditService)
        {
            _skillRepository = skillRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(CreateSkillDto dto, long adminUserId)
        {
            if (await _skillRepository.ExistsByNameAsync(dto.Name))
            {
                return (false, $"Thẻ kỹ năng '{dto.Name}' đã tồn tại trong hệ thống.");
            }

            var skill = new Skill
            {
                Name = dto.Name.Trim(),
                IndustryId = dto.IndustryId,
                Category = dto.Category?.Trim(),
                IsHot = dto.IsHot,
                IsActive = true
            };

            await _skillRepository.AddAsync(skill);

            await _auditService.LogAsync(
                userId: adminUserId,
                action: "SKILL_CREATED",
                tableName: "catalog.skills",
                recordId: skill.Id,
                description: $"Tạo thẻ kỹ năng mới: '{skill.Name}'"
            );

            return (true, $"Đã thêm thẻ kỹ năng '{skill.Name}' thành công!");
        }
    }
}