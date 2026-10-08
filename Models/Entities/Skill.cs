using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Skill
{
    public int Id { get; set; }

    public int IndustryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public string? IconUrl { get; set; }

    public bool IsHot { get; set; }

    public bool IsActive { get; set; }

    public virtual Industry Industry { get; set; } = null!;

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
}
