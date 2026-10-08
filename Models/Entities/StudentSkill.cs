using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class StudentSkill
{
    public long StudentId { get; set; }

    public int SkillId { get; set; }

    public byte ProficiencyLevel { get; set; }

    public decimal YearsOfExperience { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual StudentProfile Student { get; set; } = null!;
}
