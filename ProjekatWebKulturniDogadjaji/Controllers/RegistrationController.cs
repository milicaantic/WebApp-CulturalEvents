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
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId)
        {
            var ev = await _context.Events
                        .Include(e => e.EventRegistrations)
                        .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var already = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == user.Id);
            if (already != null)
            {
                TempData["Info"] = "You are already registered for this event.";
                return RedirectToAction("Details", "Event", new { id = eventId });
            }

            var reg = new EventRegistration
            {
                EventId = eventId,
                UserId = user.Id
            };

            _context.EventRegistrations.Add(reg);
            await _context.SaveChangesAsync();

            TempData["Success"] = "You have successfully registered.";
            return RedirectToAction("Details", "Event", new { id = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unregister(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var reg = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == user.Id);

            if (reg != null)
            {
                _context.EventRegistrations.Remove(reg);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Your registration has been successfully canceled.";
            return RedirectToAction("Details", "Event", new { id = eventId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUser(int eventId, string userId)
        {
            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (registration != null)
            {
                _context.EventRegistrations.Remove(registration);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The user has been removed from the event.";
            }
            else
            {
                TempData["Error"] = "User not found.";
            }

            return RedirectToAction("Details", "Event", new { id = eventId });
        }
    }
}
