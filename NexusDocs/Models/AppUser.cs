using Microsoft.AspNetCore.Identity;


namespace NexusDocs.Models
{
    public class AppUser : IdentityUser
    {
        public required string Email  { get; set; }

        public string? GoogleAuthToken { get; set; }

        public string? UserKey { get; set; }

        public DateTime Birthday { get; set; }
    }
}
