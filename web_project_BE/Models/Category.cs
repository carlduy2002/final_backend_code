using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int category_id { get; set; } 

        [Required(ErrorMessage = "Please, enter category name!")]
        [Column(TypeName = "varchar(30)")]
        public string category_name { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? category_description { get; set;}

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string category_status { get; set; }

        public virtual ICollection<Product>? products { get; set; }
    }

    public enum category_status
    {
        Yes, No, New, Waiting_deleted
    }
}
