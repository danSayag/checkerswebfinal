using System.ComponentModel.DataAnnotations;

namespace CheckersWeb.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required, StringLength(100),]
        public string Password { get; set; } = string.Empty;

        public string? Name { get; set; }

        public User() { }

        public User(string name, string password, string username)
        {
            Username = username;
            Password = password;
            Name = name;
        }
    }
}

