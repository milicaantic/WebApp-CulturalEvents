using System;
using System.ComponentModel.DataAnnotations;

namespace ProjekatWebKulturniDogadjaji.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

       
    }
}
