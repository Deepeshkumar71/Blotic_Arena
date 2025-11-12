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

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("role")]
        public string? Role { get; set; }

        [Column("bio")]
        public string? Bio { get; set; }

        [Column("instagram_url")]
        public string? InstagramUrl { get; set; }

        [Column("linkedin_url")]
        public string? LinkedinUrl { get; set; }

        [Column("whatsapp_url")]
        public string? WhatsappUrl { get; set; }

        [Column("github_url")]
        public string? GithubUrl { get; set; }

        [Column("is_leadership")]
        public bool IsLeadership { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("display_order")]
        public int? DisplayOrder { get; set; }

        [Column("skills")]
        public object? Skills { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
