using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Size
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int size_id { get; set; }

        [Required]
        public int size_number { get; set;}

        [Required]
        [Column(TypeName = "varchar(5)")]
        public string size_status { get; set; } = "Yes";

        public virtual ICollection<Product>? product { get; set; }
    }

    public enum size_status
    {
        Yes, No
    }
}
