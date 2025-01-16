using System.ComponentModel.DataAnnotations;

namespace BookToFlyAPI.Models{
    public class User{

        // public int Id{get;set;}
        [Key]
        [Required]
        public string Username{get; set;}
        [Required]
        public string Password{get; set;}
    }
}