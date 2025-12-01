using System;
using System.Collections.Generic;

namespace QuanLyThueXe.Models;

public partial class Driver
{
    public int DriverId { get; set; }

    public string? FullName { get; set; }

    public string? Gender { get; set; }

    public int? Age { get; set; }

    public string? Phone { get; set; }

    public int? ExperienceYears { get; set; }

    public decimal? PricePerDay { get; set; }

    public string? ImageUrl { get; set; }

    public string? Status { get; set; }

    public string? Description { get; set; }

    public string? LicenseType { get; set; }

    public bool? IsAvailable { get; set; } = false;
}
