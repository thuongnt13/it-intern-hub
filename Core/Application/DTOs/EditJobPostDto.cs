using System.ComponentModel.DataAnnotations;

namespace ITInternHub.Core.Application.DTOs
{
    public class EditJobPostDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài đăng")]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int IndustryId { get; set; }

        public byte WorkingMode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ làm việc")]
        public string WorkingAddress { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = "Hà Nội";

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool IsUnpaid { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu")]
        public string Requirements { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập quyền lợi")]
        public string Benefits { get; set; } = string.Empty;

        public byte Status { get; set; }
        public string? RejectionReason { get; set; }

        public List<JobSkillItemDto> SelectedSkills { get; set; } = new();
    }
}