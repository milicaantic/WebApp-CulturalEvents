using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjekatWebKulturniDogadjaji.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string? Description { get; set; }

        [Required, StringLength(300)]
        public string? Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;

        public double AverageRating { get; set; } = 0.0;
        [Required]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? CreatorId { get; set; }
        public ApplicationUser? Creator { get; set; }

        public ICollection<EventRegistration>? EventRegistrations { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}
