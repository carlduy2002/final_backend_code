using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Order_Detail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int od_id { get; set; }

        [Required]
        public int od_quantity { get; set; }

        [Required]
        public double od_product_price { get; set; }

        [ForeignKey("order")]
        public int od_order_id { get; set; }
        public virtual Order? order { get; set; }

        [ForeignKey("product")]
        public int od_product_id { get; set; }
        public virtual Product? product { get; set; }
    }
}
