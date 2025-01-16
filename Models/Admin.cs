using System.ComponentModel.DataAnnotations;

namespace BookToFlyAPI.Models
{
    public class Admin
    {
        [Key]
        [Required]
        public string AdminName{get; set;}
        [Required]
        public string Password{get; set;}
        
    }
}