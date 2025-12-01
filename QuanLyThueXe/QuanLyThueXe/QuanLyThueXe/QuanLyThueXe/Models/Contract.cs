using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThueXe.Models;

public partial class Contract
{
    public int ContractId { get; set; }

    public int? CarId { get; set; }

    public int CustomerId { get; set; }

    public DateTime? ContractDate { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? Deposit { get; set; }

    public string? Status { get; set; }

    public int Quantity { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
    [NotMapped]
    public string VehicleType { get; set; }
}
