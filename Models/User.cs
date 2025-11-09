using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BloticArena.Models
{
    /// <summary>
    /// User model for Supabase authentication and game tracking
    /// </summary>
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        // Client-side only properties (not in database)
        public int GamesRemaining { get; set; }
        public DateTime LastSync { get; set; }
    }
}
