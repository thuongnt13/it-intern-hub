using ITInternHub.Core.Application.Contracts.Repositories;
using ITInternHub.Services;

namespace ITInternHub.Core.Application.UseCases.Skills
{
    public class DeleteSkillUseCase
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IAuditService _auditService;

        public DeleteSkillUseCase(ISkillRepository skillRepository, IAuditService auditService)
        {
            _skillRepository = skillRepository;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(int id, long adminUserId)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null)
            {
                return (false, "Không tìm thấy thẻ kỹ năng.");
            }

            // KIỂM TRA RÀNG BUỘC TOÀN VẸN MIỀN: Nếu đang có tin tuyển dụng hoặc sinh viên chọn thì KHÔNG ĐƯỢC XÓA
            if (await _skillRepository.IsSkillInUseAsync(id))
            {
                return (false, $"Kỹ năng '{skill.Name}' đang được các tin tuyển dụng hoặc sinh viên sử dụng, không thể xóa trực tiếp. Bạn chỉ có thể chỉnh sửa hoặc tắt hiển thị!");
            }

            await _skillRepository.DeleteAsync(skill);

            await _auditService.LogAsync(
                userId: adminUserId,
                action: "SKILL_DELETED",
                tableName: "catalog.skills",
                recordId: id,
                description: $"Xóa thẻ kỹ năng: '{skill.Name}'"
            );

            return (true, $"Đã xóa thẻ kỹ năng '{skill.Name}' thành công!");
        }
    }
}