using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BloticArena.Models
{
    /// <summary>
    /// Game session model for tracking user game allowances
    /// </summary>
    [Table("game_sessions")]
    public class GameSession : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("session_token")]
        public string? SessionToken { get; set; }

        [Column("games_remaining")]
        public int GamesRemaining { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
