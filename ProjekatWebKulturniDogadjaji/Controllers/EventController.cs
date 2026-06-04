using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjekatWebKulturniDogadjaji.Data;
using ProjekatWebKulturniDogadjaji.Models;
using ProjekatWebKulturniDogadjaji.Services;

namespace ProjekatWebKulturniDogadjaji.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WeatherService _weatherService;

        public EventController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, WeatherService weatherService)
        {
            _context = context;
            _userManager = userManager;
            _weatherService = weatherService;
        }

        [AllowAnonymous]
        public IActionResult Index() => RedirectToAction(nameof(PublicIndex));

        [AllowAnonymous]
        public async Task<IActionResult> PublicIndex(int? categoryId, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.Events.Include(e => e.Category)
                        .Where(e => e.IsApproved && e.Category.IsApproved);

            if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);
            if (dateFrom.HasValue) query = query.Where(e => e.StartDate >= dateFrom.Value);
            if (dateTo.HasValue) query = query.Where(e => e.StartDate <= dateTo.Value);
            ViewBag.SelectedCategoryId = categoryId?.ToString();

            var events = await query.OrderBy(e => e.StartDate).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsApproved)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(events);
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                        .Include(e => e.Category)
                        .Include(e => e.EventRegistrations)
                        .ThenInclude(r => r.User)
                        .Include(e => e.Comments)
                        .Include(e => e.Ratings)
                        .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();
          
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
            var weather = await _weatherService.GetWeatherAsync(ev.Location ?? "");
            ViewData["Weather"] = weather;

            return View(ev);
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories
                                           .Where(c => c.IsApproved)
                                           .ToListAsync();

            if (!categories.Any())
            {
                TempData["Error"] = "There are currently no approved categories. Event creation is not possible until an admin approves categories.";
                return RedirectToAction("Index");
            }

            ViewData["Categories"] = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create([Bind("Title,Description,Location,StartDate,CategoryId")] Event evt)
        {
            var categories = await _context.Categories
                                           .Where(c => c.IsApproved)
                                           .ToListAsync();

            if (!categories.Any())
            {
                TempData["Error"] = "There are currently no approved categories. Event creation is not possible until an admin approves categories.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = new SelectList(categories, "Id", "Name", evt.CategoryId);
                return View(evt);
            }

            var user = await _userManager.GetUserAsync(User);
            evt.CreatorId = user.Id;
            evt.Creator = user;
            evt.IsApproved = await _userManager.IsInRoleAsync(user, "Admin");

            _context.Add(evt);
            await _context.SaveChangesAsync();

            TempData["Success"] = "The event has been successfully created!";
            return RedirectToAction("MyEvents");
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            if (ev.CreatorId != user?.Id && !isAdmin) return Forbid();

            var categories = await _context.Categories.Where(c => c.IsApproved).ToListAsync();

            if (ev.CategoryId != 0 && !categories.Any(c => c.Id == ev.CategoryId))
            {
                var currentCategory = await _context.Categories.FindAsync(ev.CategoryId);
                if (currentCategory != null)
                {
                    categories.Add(currentCategory);
                }
            }

            ViewData["Categories"] = new SelectList(categories, "Id", "Name", ev.CategoryId);
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Location,StartDate,CategoryId,IsApproved")] Event evt)
        {
            if (id != evt.Id) return NotFound();

            var existing = await _context.Events.FindAsync(id);
            if (existing == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            if (existing.CreatorId != user?.Id && !isAdmin) return Forbid();

            var categories = await _context.Categories.Where(c => c.IsApproved).ToListAsync();

            if (!categories.Any(c => c.Id == existing.CategoryId))
            {
                var currentCategory = await _context.Categories.FindAsync(existing.CategoryId);
                if (currentCategory != null) categories.Add(currentCategory);
            }

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = new SelectList(categories, "Id", "Name", evt.CategoryId);
                return View(evt);
            }

            existing.Title = evt.Title;
            existing.Description = evt.Description;
            existing.Location = evt.Location;
            existing.StartDate = evt.StartDate;
            existing.CategoryId = evt.CategoryId;

            if (isAdmin) existing.IsApproved = evt.IsApproved;

            _context.Update(existing);
            await _context.SaveChangesAsync();

            TempData["Success"] = "The event has been successfully updated.";
            return RedirectToAction(nameof(PublicIndex));
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();

            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The event has been successfully deleted.";
            }
            return RedirectToAction(nameof(PublicIndex));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var pending = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Creator)
                .Where(e => !e.IsApproved)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var ev = await _context.Events.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();

            if (!ev.Category.IsApproved)
            {
                TempData["Error"] = $"You cannot approve the event '{ev.Title}' because its category '{ev.Category.Name}' is not approved.";
                return RedirectToAction(nameof(Pending));
            }

            ev.IsApproved = true;
            _context.Update(ev);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"The event '{ev.Title}' has been successfully approved.";
            return RedirectToAction(nameof(Pending));
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> MyEvents()
        {
            var userId = _userManager.GetUserId(User);
            var myEvents = await _context.Events
                .Include(e => e.Category)
                .Where(e => e.CreatorId == userId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            return View(myEvents);
        }
    }
}
