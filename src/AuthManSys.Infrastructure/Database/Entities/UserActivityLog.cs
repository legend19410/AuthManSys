using System.ComponentModel.DataAnnotations;
using AuthManSys.Domain.Enums;

namespace AuthManSys.Infrastructure.Database.Entities
{
    public class UserActivityLog
    {
        [Key]
        public long Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        public ActivityEventType EventType { get; set; }

        [MaxLength(100)]
        public string? EventTag { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [MaxLength(100)]
        public string? IPAddress { get; set; }

        [MaxLength(200)]
        public string? Device { get; set; }

        [MaxLength(100)]
        public string? Platform { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public string? Metadata { get; set; } // JSON blob for extra info
    }
}