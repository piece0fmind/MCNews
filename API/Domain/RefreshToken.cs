using API.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Domain
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        [ForeignKey("AppUser")]
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public AppUser User { get; set; }

    }
}
