using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class StatusLog
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public byte PreviousStatus { get; set; }

    public byte NewStatus { get; set; }

    public long ChangedByUserId { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual User ChangedByUser { get; set; } = null!;
}
