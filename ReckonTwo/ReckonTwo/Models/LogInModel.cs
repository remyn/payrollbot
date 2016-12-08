using System.ComponentModel.DataAnnotations;

namespace ReckonTwo.Models
{
    public class LogInModel
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Please provide a valid email address")]
        public string Email { get; set; }

        [Required]
        [MaxLength(550)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}