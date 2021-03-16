using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RestApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [DisplayName("Kategori")]
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string CategoryName { get; set; }

        //FK for Product
        public ICollection<Product> Products { get; set; }

        public Category()
        {
        }
    }
}
