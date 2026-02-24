using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjekatWebKulturniDogadjaji.Models;

namespace ProjekatWebKulturniDogadjaji.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

       
            builder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Category>()
                   .HasIndex(c => c.Name)
                   .IsUnique();

            builder.Entity<Event>()
                   .Property(e => e.AverageRating)
                   .HasColumnType("float");


            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comment>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Comments)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

         
            builder.Entity<EventRegistration>()
                .HasOne(r => r.Event)
                .WithMany(e => e.EventRegistrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventRegistration>()
                .HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Rating>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Ratings)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

       
            builder.Entity<Rating>()
                .HasIndex(r => new { r.EventId, r.UserId })
                .IsUnique();

         
            builder.Entity<EventRegistration>()
                .HasIndex(r => new { r.EventId, r.UserId })
                .IsUnique();

          
        }
    }
}
