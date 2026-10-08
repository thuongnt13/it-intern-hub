using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Permission
{
    public int Id { get; set; }

    public string Module { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
