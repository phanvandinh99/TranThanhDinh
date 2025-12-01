using System;
using System.Collections.Generic;

namespace QuanLyThueXe.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? IdentityCard { get; set; }

    public int? CarId { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public virtual Car? Car { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
