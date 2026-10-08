using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class StudentProfile
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public string Major { get; set; } = null!;

    public int AdmissionYear { get; set; }

    public decimal? Gpa { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public byte Gender { get; set; }

    public string? CvUrl { get; set; }

    public string? GithubUrl { get; set; }

    public string? LinkedinUrl { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? Bio { get; set; }

    public byte JobSeekingStatus { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();

    public virtual User User { get; set; } = null!;
}
