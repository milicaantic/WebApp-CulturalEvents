using System;
using System.ComponentModel.DataAnnotations;

namespace ProjekatWebKulturniDogadjaji.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required, StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}
