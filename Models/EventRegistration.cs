using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BloticArena.Models
{
    /// <summary>
    /// Event registration model from website database
    /// </summary>
    [Table("event_registrations")]
    public class EventRegistration : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("event_id")]
        public Guid EventId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("games_remaining")]
        public int GamesRemaining { get; set; }

        [Column("payment_status")]
        public string? PaymentStatus { get; set; }

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; }

        [Column("status")]
        public string? Status { get; set; }
    }
}
