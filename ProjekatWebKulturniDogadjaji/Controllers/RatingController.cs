using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjekatWebKulturniDogadjaji.Data;
using ProjekatWebKulturniDogadjaji.Models;

namespace ProjekatWebKulturniDogadjaji.Controllers
{
    [Authorize]
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId, int score)
        {
            

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var existing = await _context.Ratings.FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == user.Id);
            if (existing != null)
            {
                existing.Score = score;
                _context.Ratings.Update(existing);
            }
            else
            {
                var rating = new Rating { EventId = eventId, UserId = user.Id, Score = score };
                _context.Ratings.Add(rating);
            }

            await _context.SaveChangesAsync();

            var avg = await _context.Ratings
                .Where(r => r.EventId == eventId)
                .AverageAsync(r => (double)r.Score);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev != null)
            {
                ev.AverageRating = System.Math.Round(avg, 2);
                _context.Update(ev);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "The rating has been successfully recorded.";
            return RedirectToAction("Details", "Event", new { id = eventId });
        }
    }
}
