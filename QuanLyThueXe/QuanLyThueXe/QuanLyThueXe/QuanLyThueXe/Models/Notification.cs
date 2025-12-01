using System;
using System.Collections.Generic;

namespace QuanLyThueXe.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsRead { get; set; }

    public int? ReservationId { get; set; }

    public virtual Reservation? Reservation { get; set; }
}
