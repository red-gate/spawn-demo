using System;

namespace Spawn.Demo.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Task { get; set; }
        public bool Done { get; set; }
        public int? ProjectId { get; set; }
    }
}