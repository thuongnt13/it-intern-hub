using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class JobSkill
{
    public long JobPostId { get; set; }

    public int SkillId { get; set; }

    public byte PriorityLevel { get; set; }

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
