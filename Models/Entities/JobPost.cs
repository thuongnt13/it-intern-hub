using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class JobPost
{
    public long Id { get; set; }

    public long CompanyId { get; set; }

    public int IndustryId { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int Quantity { get; set; }

    public byte WorkingMode { get; set; }

    public string WorkingAddress { get; set; } = null!;

    public string City { get; set; } = null!;

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public bool IsUnpaid { get; set; }

    public DateOnly Deadline { get; set; }

    public string Description { get; set; } = null!;

    public string Requirements { get; set; } = null!;

    public string Benefits { get; set; } = null!;

    public byte Status { get; set; }

    public string? RejectionReason { get; set; }

    public long ViewCount { get; set; }

    public int ApplicationCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual CompanyProfile Company { get; set; } = null!;

    public virtual Industry Industry { get; set; } = null!;

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
