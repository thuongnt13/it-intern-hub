using ITInternHub.Core.Domain.Exceptions;

namespace ITInternHub.Models.Entities
{
    // Rich Domain Model cho Thực thể Kỹ năng
    public partial class Skill
    {
        public void UpdateDetails(string name, int industryId, string? category, bool isHot)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Tên kỹ năng không được để trống.");
            }

            this.Name = name.Trim();
            this.IndustryId = industryId;
            this.Category = category?.Trim();
            this.IsHot = isHot;
        }

        public void ToggleHot()
        {
            this.IsHot = !this.IsHot;
        }

        public void SetActive(bool isActive)
        {
            this.IsActive = isActive;
        }
    }
}