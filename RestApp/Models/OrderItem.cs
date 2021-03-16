using System;
using System.ComponentModel;

namespace RestApp.Models
{
    public class OrderItem
    {
        // PK
        public int Id { get; set; }

        //FK for Order
        [DisplayName("Beställning Nr")]
        public int OrderId { get; set; }
        [DisplayName("Beställning")]
        public Order Order { get; set; }

        //FK for Product
        [DisplayName("Produkt")]
        public int ProductId { get; set; }
        [DisplayName("Produkt")]
        public Product Product { get; set; }

        public OrderItem()
        {
        }

        public static implicit operator int(OrderItem v)
        {
            throw new NotImplementedException();
        }
    }
}
