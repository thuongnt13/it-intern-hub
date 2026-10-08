using System.ComponentModel.DataAnnotations;

namespace ITInternHub.Core.Application.DTOs
{
    public class CreateSkillDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên kỹ năng")]
        [StringLength(100, ErrorMessage = "Tên kỹ năng tối đa 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn lĩnh vực")]
        public int IndustryId { get; set; }

        public string? Category { get; set; }
        public bool IsHot { get; set; }
    }

    public class UpdateSkillDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên kỹ năng")]
        [StringLength(100, ErrorMessage = "Tên kỹ năng tối đa 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn lĩnh vực")]
        public int IndustryId { get; set; }

        public string? Category { get; set; }
        public bool IsHot { get; set; }
    }
}