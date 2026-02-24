using Microsoft.AspNetCore.Mvc;
using ProjekatWebKulturniDogadjaji.Data;
using ProjekatWebKulturniDogadjaji.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ProjekatWebKulturniDogadjaji.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create(int eventId, string content)
        {
            if (!User.Identity.IsAuthenticated) return Forbid();

            var existingEvent = await _context.Events.FindAsync(eventId);
            if (existingEvent == null) return NotFound();

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Komentar ne može biti prazan.";
                return RedirectToAction("Details", "Event", new { id = eventId });
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var comment = new Comment
                {
                    EventId = eventId,
                    Content = content,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Komentar je uspešno dodat!";

            return RedirectToAction("Details", "Event", new { id = eventId });
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != userId && !User.IsInRole("Admin")) return Forbid();

            return View(comment);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Komentar je uspešno obrisan.";
            return RedirectToAction("Details", "Event", new { id = comment.EventId });
        }
    }
}
