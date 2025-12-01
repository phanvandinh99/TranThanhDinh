using System;
using System.Collections.Generic;

namespace QuanLyThueXe.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int? CarId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? ReservationDate { get; set; }

    public string? Status { get; set; }

    public virtual Car? Car { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
