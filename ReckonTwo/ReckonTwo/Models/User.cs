using System;
using System.ComponentModel.DataAnnotations;

namespace ReckonTwo.Models
{
    public class User
    {
        public Guid UserID { get; set; }

        [Required]
        [MaxLength(150)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(150)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get { return string.Concat(FirstName, " ", LastName); } }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Please provide a valid email address")]
        [MaxLength(250)]
        public string Email { get; set; }

        [Required]
        [MaxLength(550)]
        public string Password { get; set; }

        public bool FlagDeleted { get; set; }
        public bool FlagAdmin { get; set; }
    }
}