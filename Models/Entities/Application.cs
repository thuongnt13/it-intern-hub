using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Application
{
    public long Id { get; set; }

    public long JobPostId { get; set; }

    public long StudentId { get; set; }

    public string CvUrl { get; set; } = null!;

    public string? CoverLetter { get; set; }

    public decimal MatchScore { get; set; }

    public byte Status { get; set; }

    public string? RecruiterNote { get; set; }

    public DateTime AppliedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    public virtual ICollection<StatusLog> StatusLogs { get; set; } = new List<StatusLog>();

    public virtual StudentProfile Student { get; set; } = null!;
}
