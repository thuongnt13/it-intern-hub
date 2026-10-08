using System.ComponentModel.DataAnnotations;

namespace ITInternHub.Core.Application.DTOs
{
    public class JobSkillItemDto
    {
        public int SkillId { get; set; }
        public byte PriorityLevel { get; set; } // 1: Required (Bắt buộc), 2: Preferred (Ưu tiên)
    }

    public class CreateJobPostDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài đăng")]
        [Display(Name = "Tiêu đề tuyển dụng")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn lĩnh vực")]
        public int IndustryId { get; set; }

        [Required]
        public byte WorkingMode { get; set; } = 1; // 1: Onsite, 2: Remote, 3: Hybrid

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ làm việc")]
        public string WorkingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố")]
        public string City { get; set; } = "Hà Nội";

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool IsUnpaid { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn nộp hồ sơ")]
        public DateTime Deadline { get; set; } = DateTime.Now.AddMonths(1);

        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Vui lòng nhập mô tả công việc")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu ứng viên")]
        public string Requirements { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập quyền lợi")]
        public string Benefits { get; set; } = string.Empty;

        // Danh sách thẻ kỹ năng được chọn đính kèm mức ưu tiên
        public List<JobSkillItemDto> SelectedSkills { get; set; } = new();
    }
}