using ITInternHub.Models.Entities;

namespace ITInternHub.Models.ViewModels
{
    public class JobApprovalIndexViewModel
    {
        // Danh sách tin chờ duyệt (Tab 1)
        public List<JobPost> PendingJobs { get; set; } = new();

        // Danh sách tin đã duyệt công khai (Tab 2)
        public List<JobPost> PublishedJobs { get; set; } = new();
    }
}