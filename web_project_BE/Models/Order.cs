using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int order_id { get; set; }

        [Required]
        public DateTime order_date { get; set; } = DateTime.Now;
        public DateTime? delivery_date { get; set; } = null;

        [Required(ErrorMessage = "Please, enter the your addess!")]
        [Column(TypeName = "varchar(255)")]
        public string order_address { get; set; }

        [Required(ErrorMessage = "Please, enter the your phone!")]
        [Column(TypeName = "varchar(10)")]
        public string order_phone { get; set; }

        [Required]
        public int order_quantity { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? order_note { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string order_status { get; set; } = "Pending";

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string order_payment { get; set; }

        [Required]
        public double order_total_price { get; set; }

        [ForeignKey("account")]
        public int o_account_id { get; set; }
        public virtual Account? account { get; set; }
    }

    enum order_status
    {
        Pending, Awaiting_Pickup, Delivered, Cancelled, Returned, Rejected
    }
}
