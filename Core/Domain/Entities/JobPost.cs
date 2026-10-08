using ITInternHub.Core.Domain.Enums;
using ITInternHub.Core.Domain.Exceptions;

namespace ITInternHub.Models.Entities
{
    // Rich Domain Model: Chứa toàn bộ nghiệp vụ kiểm soát trạng thái của tin tuyển dụng
    public partial class JobPost
    {
        // Nghiệp vụ 1: Admin Phê duyệt bài đăng
        public void Approve()
        {
            // Kiểm tra ràng buộc bất biến (Business Invariant)
            if (this.Status != (byte)JobStatus.PendingApproval)
            {
                throw new DomainException($"Chỉ có thể phê duyệt bài đăng đang ở trạng thái 'Chờ duyệt'. Trạng thái hiện tại: {this.Status}");
            }

            this.Status = (byte)JobStatus.Published;
            this.UpdatedAt = DateTime.UtcNow;
            this.RejectionReason = null; // Xóa lý do từ chối cũ nếu có
        }

        // Nghiệp vụ 2: Admin Từ chối bài đăng kèm lý do
        public void Reject(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new DomainException("Bắt buộc phải nhập lý do khi từ chối bài đăng tuyển dụng.");
            }

            if (this.Status != (byte)JobStatus.PendingApproval)
            {
                throw new DomainException("Chỉ có thể từ chối bài đăng đang chờ xét duyệt.");
            }

            this.Status = (byte)JobStatus.Rejected;
            this.RejectionReason = reason.Trim();
            this.UpdatedAt = DateTime.UtcNow;
        }

        // Nghiệp vụ 3: Đóng tin tuyển dụng khi hết hạn hoặc đủ người
        public void Close()
        {
            if (this.Status != (byte)JobStatus.Published)
            {
                throw new DomainException("Chỉ có thể đóng bài đăng đang hiển thị tuyển dụng.");
            }

            this.Status = (byte)JobStatus.Closed;
            this.UpdatedAt = DateTime.UtcNow;
        }

        // NGHIỆP VỤ 4: CHỈNH SỬA TIN TUYỂN DỤNG CỦA DOANH NGHIỆP
        public void UpdateDetails(
            string title,
            int industryId,
            byte workingMode,
            string workingAddress,
            string city,
            decimal? salaryMin,
            decimal? salaryMax,
            bool isUnpaid,
            DateOnly deadline,
            int quantity,
            string description,
            string requirements,
            string benefits)
        {
            // 1. RÀNG BUỘC: Đã duyệt thì không cho sửa
            if (this.Status == (byte)JobStatus.Published)
            {
                throw new DomainException("Tin tuyển dụng đã được Ban quản trị phê duyệt hiển thị, không thể chỉnh sửa.");
            }

            if (this.Status == (byte)JobStatus.Closed)
            {
                throw new DomainException("Tin tuyển dụng đã đóng, không thể chỉnh sửa.");
            }

            // 2. CẬP NHẬT DỮ LIỆU
            this.Title = title.Trim();
            this.IndustryId = industryId;
            this.WorkingMode = workingMode;
            this.WorkingAddress = workingAddress.Trim();
            this.City = city.Trim();
            this.SalaryMin = isUnpaid ? null : salaryMin;
            this.SalaryMax = isUnpaid ? null : salaryMax;
            this.IsUnpaid = isUnpaid;
            this.Deadline = deadline;
            this.Quantity = quantity;
            this.Description = description;
            this.Requirements = requirements;
            this.Benefits = benefits;
            this.UpdatedAt = DateTime.UtcNow;

            // 3. NẾU TIN ĐANG BỊ TỪ CHỐI -> GỬI LẠI SẼ TỰ ĐỘNG CHUYỂN SANG CHỜ DUYỆT
            if (this.Status == (byte)JobStatus.Rejected)
            {
                this.Status = (byte)JobStatus.PendingApproval;
                this.RejectionReason = null; // Xóa lý do từ chối cũ
            }
        }

    }
}