using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Assignment3.Models;

namespace Assignment3.Controllers
{
    [Authorize(Roles = "Admin,EventManager")]
    [Route("EventCategories")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.EventCategories
                .Include(c => c.Events)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            return View(categories);
        }

        // GET: Categories/Details/5
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.EventCategories
                .Include(c => c.Events)
                .ThenInclude(e => e.Attendees)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Categories/Create
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName")] EventCategory category)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate category names
                var existingCategory = await _context.EventCategories
                    .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == category.CategoryName.ToLower());

                if (existingCategory != null)
                {
                    ModelState.AddModelError("CategoryName", "A category with this name already exists.");
                    return View(category);
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.EventCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,CategoryName")] EventCategory category)
        {
            if (id != category.CategoryID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate category names (excluding current category)
                    var existingCategory = await _context.EventCategories
                        .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == category.CategoryName.ToLower() 
                                                && c.CategoryID != category.CategoryID);

                    if (existingCategory != null)
                    {
                        ModelState.AddModelError("CategoryName", "A category with this name already exists.");
                        return View(category);
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Category updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.EventCategories
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.EventCategories
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category != null)
            {
                // Check if category has associated events
                if (category.Events.Any())
                {
                    TempData["Error"] = "Cannot delete category because it has associated events.";
                    return RedirectToAction(nameof(Index));
                }

                _context.EventCategories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.EventCategories.Any(e => e.CategoryID == id);
        }
    }
} 