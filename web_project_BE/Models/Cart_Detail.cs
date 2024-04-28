using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Cart_Detail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int cd_id { get; set; }

        [Required]
        public int cd_quantity { get; set; } = 1;

        [ForeignKey("cart")]
        public int cd_cart_id { get; set; }
        public virtual Cart? cart { get; set; }

        [ForeignKey("product")]
        public int cd_product_id { get; set; }
        public virtual Product? product { get; set; }
    }
}
