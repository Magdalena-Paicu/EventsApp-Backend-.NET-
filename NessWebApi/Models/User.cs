using NessWebApi.Data;
namespace NessWebApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public string ImageUrl { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime ResetPasswordExpiry { get; set; }
        public List<Event>FavoriteEvents { get; set; } = new List<Event>();

    }
}