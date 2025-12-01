using System;
using QuanLyThueXe.Models;

namespace QuanLyThueXe.ViewModels
{
    public class CustomerContractVM
    {
        public Customer Customer { get; set; }
        public Contract ActiveContract { get; set; }

        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string IdentityCard { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }

        public string CarModel { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
