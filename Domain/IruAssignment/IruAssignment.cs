using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.IruAssignment
{
    public class IruAssignment
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string ItemCode { get; set; }
        public string ColorCode { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountPrice { get; set; }
        public string DeliveredIn { get; set; }
        public string Q1 { get; set; }
        public int Size { get; set; }
        public string Color { get; set; }


    }
}