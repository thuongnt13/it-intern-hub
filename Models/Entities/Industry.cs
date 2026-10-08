using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Industry
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
