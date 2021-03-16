using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RestApp.Models
{
    public class Order
    {
        // PK
        public int Id { get; set; }

        public DateTime OrderTime { get; set; } = DateTime.Now;

        [DisplayName("Email")]
        public string UserEmail { get; set; }

        [DisplayName("Bordsnummer")]
        public string Table { get; set; }

        public bool TakeAway { get; set; }

        public bool IsSent { get; set; } = false;

        public bool IsPayed { get; set; } = false;

        //FK for orderitem
        public ICollection<OrderItem> OrderItems { get; set; }
        public Order()
        {
        }
    }
}
