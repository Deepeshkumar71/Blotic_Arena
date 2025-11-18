using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BloticArena.Models
{
    /// <summary>
    /// Event model for event management
    /// </summary>
    [Table("events")]
    public class Event : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("event_date")]
        public DateTime EventDate { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("max_participants")]
        public int? MaxParticipants { get; set; }

        [Column("current_participants")]
        public int? CurrentParticipants { get; set; }

        [Column("is_registration_open")]
        public bool IsRegistrationOpen { get; set; }

        [Column("registration_deadline")]
        public DateTime? RegistrationDeadline { get; set; }

        [Column("event_type")]
        public string? EventType { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("is_featured")]
        public bool IsFeatured { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("is_paid_event")]
        public bool IsPaidEvent { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("registration_fee")]
        public decimal? RegistrationFee { get; set; }

        [Column("requirements")]
        public string? Requirements { get; set; }

        [Column("event_link")]
        public string? EventLink { get; set; }

        [Column("number_of_games")]
        public int? NumberOfGames { get; set; }

        [Column("payment_registration_fee")]
        public decimal? PaymentRegistrationFee { get; set; }
    }
}
