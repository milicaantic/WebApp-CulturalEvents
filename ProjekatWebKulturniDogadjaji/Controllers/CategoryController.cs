using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjekatWebKulturniDogadjaji.Data;
using ProjekatWebKulturniDogadjaji.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProjekatWebKulturniDogadjaji.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var all = await _context.Categories.ToListAsync();
            return View(all);
        }

        public IActionResult Create() => View();

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The category has been successfully created.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id.Value);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The category has been successfully updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            if (category == null) return NotFound();

            if (category.Events != null && category.Events.Any())
            {
                TempData["Error"] = "You cannot delete a category that has events.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            if (category == null) return NotFound();

            if (category.Events != null && category.Events.Any())
            {
                TempData["Error"] = "You cannot delete a category that has events.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "The category has been deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id.Value);
            if (category == null) return NotFound();

            category.IsApproved = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"The category '{category.Name}' has been approved.";
            return RedirectToAction(nameof(Index));
        }
    }
}
