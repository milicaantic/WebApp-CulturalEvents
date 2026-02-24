using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjekatWebKulturniDogadjaji.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
