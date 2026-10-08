using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class UniversityOfficer
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string OfficerCode { get; set; } = null!;

    public string FacultyDepartment { get; set; } = null!;

    public string PositionTitle { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
