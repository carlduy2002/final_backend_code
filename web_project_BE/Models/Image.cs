using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int image_id { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string image_uri { get; set; }

        [ForeignKey("product")]
        public int i_product_id { get; set;}
        public virtual Product? product { get; set; }
    }
}
