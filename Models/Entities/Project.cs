using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Project
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public string Title { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Technologies { get; set; } = null!;

    public string? Description { get; set; }

    public string? GithubUrl { get; set; }

    public string? DemoUrl { get; set; }

    public virtual StudentProfile Student { get; set; } = null!;
}
