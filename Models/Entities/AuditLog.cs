using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class AuditLog
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Action { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public long? RecordId { get; set; }

    public string? Description { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
