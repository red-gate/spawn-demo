using System;

namespace Spawn.Demo.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? OrgId { get; set; }
    }
}