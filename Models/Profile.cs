using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BloticArena.Models
{
    /// <summary>
    /// User profile model from website database
    /// </summary>
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("role")]
        public string? Role { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
