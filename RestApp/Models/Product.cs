using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RestApp.Models
{
    public class Product
    {
        // PK
        public int Id { get; set; }

        [DisplayName("Produkt")]
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; }

        [DisplayName("Pris")]
        [Required]
        public int Price { get; set; }

        [DisplayName("Beskrivning")]
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Description { get; set; }


        //FK for OrderItem
        public ICollection<OrderItem> OrderItems { get; set; }


        //FK for Category
        [DisplayName("Kategori Nr")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public Product()
        {
        }
    }
}
