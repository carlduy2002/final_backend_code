    using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Supplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int supplier_id {  get; set; }

        [Required(ErrorMessage = "Please, enter the supplier name!")]
        [Column(TypeName = "varchar(30)")]
        public string supplier_name { get; set; }

        [Required(ErrorMessage = "Please, enter the supplier email!")]
        [Column(TypeName = "varchar(255)")]
        public string supplier_email { get; set; }

        [Required(ErrorMessage = "Please, enter the supplier address!")]
        [Column(TypeName = "varchar(255)")]
        public string supplier_address { get; set; }

        [Required(ErrorMessage = "Please, enter the supplier phone!")]
        [Column(TypeName = "varchar(10)")]
        public string supplier_phone { get; set; }

        [Required]
        [Column(TypeName = "varchar(5)")]
        public string supplier_status { get; set; }

        public virtual ICollection<Product>? products { get; set; }
    }

    public enum supplier_status
    {
        Yes, No
    }
}
