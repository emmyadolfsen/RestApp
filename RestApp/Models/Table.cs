using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RestApp.Models
{
    public class Table
    {

        public int Id { get; set; }

        [DisplayName("Bordsnummer")]
        [Required]
        public string TableName { get; set; }

        public Table()
        {
        }
    }
}
