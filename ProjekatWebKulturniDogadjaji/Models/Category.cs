using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjekatWebKulturniDogadjaji.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = true;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
