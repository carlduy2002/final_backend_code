using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int product_id { get; set; }

        [Required(ErrorMessage = "Please, enter product name!")]
        [Column(TypeName = "varchar(255)")]
        public string product_name { get; set; }

        [Required]
        public int product_quantity_stock { get; set; }

        [Required]
        public double product_original_price { get; set; }

        [Required]
        public double product_sell_price { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string? product_description { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? product_image { get; set; }

        public DateTime? product_import_date { get; set; } 

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string product_status { get; set; }

        [ForeignKey("size")]
        public int p_size_id { get; set; }
        public virtual Size? size { get; set; }

        [ForeignKey("category")]
        public int p_category_id { get; set; }
        public virtual Category? category { get; set; }

        [ForeignKey("supplier")]
        public int p_supplier_id { get; set; }
        public virtual Supplier? supplier { get; set; }

        public virtual ICollection<Order_Detail>? order_details { get; set; }
        public virtual ICollection<Image>? images { get; set; }
        public virtual ICollection<Cart_Detail>? cart_detail { get; set; }
    }

    public enum product_status
    {
        Yes, No, New, Waiting_deleted
    }
}
