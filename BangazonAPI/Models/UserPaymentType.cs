

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class UserPaymentType
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(55)]
        public string AcctNumber { get; set; }
        [Required]
        public bool Active { get; set; } = true;
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int PaymentTypeId { get; set; }

    }
}

