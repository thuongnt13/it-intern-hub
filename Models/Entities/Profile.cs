using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Profile
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string TaxCode { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? WebsiteUrl { get; set; }

    public string CompanyEmail { get; set; } = null!;

    public string? CompanyPhone { get; set; }

    public string? LogoUrl { get; set; }

    public string? EmployeeScale { get; set; }

    public string? Description { get; set; }

    public string? BusinessLicenseUrl { get; set; }

    public byte VerificationStatus { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();

    public virtual ICollection<Record> Records { get; set; } = new List<Record>();

    public virtual User User { get; set; } = null!;
}
