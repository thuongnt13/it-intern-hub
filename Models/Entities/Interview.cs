using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Interview
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public byte Format { get; set; }

    public string LocationOrLink { get; set; } = null!;

    public string? InterviewerName { get; set; }

    public string? CandidateNote { get; set; }

    public string? InterviewResult { get; set; }

    public virtual Application Application { get; set; } = null!;
}
