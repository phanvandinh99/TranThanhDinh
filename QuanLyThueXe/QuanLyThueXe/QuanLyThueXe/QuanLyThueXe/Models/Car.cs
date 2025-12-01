using System;
using System.Collections.Generic;

namespace QuanLyThueXe.Models;

public partial class Car
{
    public int CarId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public decimal? PricePerDay { get; set; }

    public string? Status { get; set; }

    public string? CarName { get; set; }

    public string? ImageUrl { get; set; }

    public string? VehicleType { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
