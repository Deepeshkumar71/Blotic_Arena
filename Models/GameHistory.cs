using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BloticArena.Models
{
    /// <summary>
    /// Game history model for tracking played games
    /// </summary>
    [Table("game_history")]
    public class GameHistory : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("game_name")]
        public string? GameName { get; set; }

        [Column("played_at")]
        public DateTime PlayedAt { get; set; }

        [Column("source")]
        public string? Source { get; set; }
    }
}
