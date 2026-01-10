using System;
using System.ComponentModel.DataAnnotations;

namespace AuthManSys.Infrastructure.Database.Entities;

public class RefreshToken
{
        [Key]
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool Used { get; set; }
        public bool Invalidated { get; set; }
        public int UserId { get; set; }
}