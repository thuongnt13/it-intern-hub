namespace ITInternHub.Models.ViewModels
{
    public class JobCardViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string City { get; set; } = string.Empty;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool IsUnpaid { get; set; }
        public DateTime Deadline { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public double MatchScore { get; set; }
    }

    public class HomeIndexViewModel
    {
        public List<JobCardViewModel> FeaturedJobs { get; set; } = new();
        public int TotalJobsCount { get; set; }
        public bool IsStudentLoggedIn { get; set; }

        // BỔ SUNG CÁC THUỘC TÍNH PHÂN TRANG
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}