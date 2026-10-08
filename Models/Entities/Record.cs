using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Record
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public long CompanyId { get; set; }

    public int InternshipTermId { get; set; }

    public long? ApplicationId { get; set; }

    public string JobTitle { get; set; } = null!;

    public string SupervisorName { get; set; } = null!;

    public string SupervisorEmail { get; set; } = null!;

    public string? SupervisorPhone { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly ExpectedEndDate { get; set; }

    public DateOnly? ActualEndDate { get; set; }

    public byte Status { get; set; }

    public string? FinalReportUrl { get; set; }

    public virtual Application? Application { get; set; }

    public virtual CompanyProfile Company { get; set; } = null!;

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public virtual Term InternshipTerm { get; set; } = null!;

    public virtual StudentProfile Student { get; set; } = null!;

    public virtual ICollection<WeeklyReport> WeeklyReports { get; set; } = new List<WeeklyReport>();
}
