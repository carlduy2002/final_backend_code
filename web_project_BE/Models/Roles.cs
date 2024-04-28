using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project_BE.Models
{
    public class Roles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int role_id {  get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string role_name { get; set;}

        public ICollection<Account>? accounts { get; set; }
    }
}
