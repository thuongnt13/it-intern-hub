namespace ITInternHub.Core.Domain.Enums
{
    public enum JobStatus : byte
    {
        Draft = 0,             // Bản nháp
        PendingApproval = 1,   // Chờ Admin duyệt
        Published = 2,         // Đang hiển thị tuyển dụng
        Closed = 3,            // Đã đóng tuyển
        Rejected = 4           // Bị Admin từ chối
    }
}