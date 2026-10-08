using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Term
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public byte Semester { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Record> Records { get; set; } = new List<Record>();
}
