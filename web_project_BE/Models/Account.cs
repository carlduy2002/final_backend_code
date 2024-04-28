using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace web_project_BE.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int account_id { get; set; }

        [Required(ErrorMessage = "Please, enter the username!")]
        [StringLength(20, ErrorMessage = "Please, the username length smaller than 20 characters!")]
        [Column(TypeName = "varchar(20)")]
        public string account_username { get; set; }

        [Required(ErrorMessage = "Please, enter the email!")]
        [Column(TypeName = "varchar(255)")]
        public string account_email { get; set;}

        [Required(ErrorMessage = "Please, enter the password!")]
        [Column(TypeName = "varchar(255)")]
        public string account_password { get; set;}

        [Required(ErrorMessage = "Please, enter the confirm password!")]
        [Column(TypeName = "varchar(255)")]
        public string account_confirm_password { get; set; }

        [Required(ErrorMessage = "Please, enter the phone number!")]
        [Column(TypeName = "varchar(10)")]
        public string account_phone { get; set;}

        [Column(TypeName = "varchar(255)")]
        public string? account_address { get; set;}

        public DateTime account_birthday { get; set; }

        [Required(ErrorMessage = "Please, choose the gender!")]
        [Column(TypeName = "varchar(20)")]
        public string account_gender { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? token { get; set;}

        [Column(TypeName = "varchar(255)")]
        public string? refesh_token { get; set;}

        public DateTime refesh_token_exprytime { get; set;}

        [Column(TypeName = "varchar(255)")]
        public string? reset_password_token { get; set; }

        public DateTime reset_password_exprytime { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string account_status { get; set; } = "Unlock";

        [Column(TypeName = "varchar(255)")]
        public string? account_avatar { get; set; }

        public int account_rejected_times { get; set; } = 0;

        [ForeignKey("role")]
        public int account_role_id { get; set; }
        public virtual Roles? role { get; set; }

        public Cart? Cart { get; set; }
        public virtual ICollection<Order>? orders { get; set; }
    }

    public enum account_status
    {
        Lock, Unlock
    }
}
