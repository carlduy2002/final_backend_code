using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace web_project_BE.Models
{
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int cart_id { get; set; }

        [ForeignKey("account_id")]
        public int account_id { get; set; }
        public Account Account { get; set; }

        public virtual ICollection<Cart_Detail>? Cart_details { get; set; }
    }
}
