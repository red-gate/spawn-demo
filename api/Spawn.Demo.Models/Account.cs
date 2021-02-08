using System;

namespace Spawn.Demo.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}