using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class WeeklyReport
{
    public long Id { get; set; }

    public long InternshipRecordId { get; set; }

    public int WeekNumber { get; set; }

    public string WorkSummary { get; set; } = null!;

    public string? SupervisorFeedback { get; set; }

    public byte Status { get; set; }

    public DateTime SubmittedAt { get; set; }

    public virtual Record InternshipRecord { get; set; } = null!;
}
