using System;
using System.Collections.Generic;

namespace ITInternHub.Models.Entities;

public partial class Notification
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? RedirectUrl { get; set; }

    public bool IsRead { get; set; }

    public string NotificationType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
