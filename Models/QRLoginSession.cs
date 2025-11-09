using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BloticArena.Models
{
    /// <summary>
    /// QR login session status
    /// </summary>
    public enum QRSessionStatus
    {
        Pending,
        Scanned,
        Authenticated,
        Expired
    }

    /// <summary>
    /// QR login session model for tracking authentication via QR code
    /// </summary>
    [Table("qr_login_sessions")]
    public class QRLoginSession : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("session_id")]
        public string? SessionId { get; set; }

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("desktop_device_id")]
        public string? DesktopDeviceId { get; set; }

        // Helper property for enum conversion (not mapped to database)
        [Newtonsoft.Json.JsonIgnore]
        public QRSessionStatus StatusEnum
        {
            get => Enum.TryParse<QRSessionStatus>(Status, true, out var result) ? result : QRSessionStatus.Pending;
            set => Status = value.ToString().ToLower();
        }
    }
}
