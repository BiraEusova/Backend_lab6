using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Microsoft.AspNetCore.Authorization;
using Backend6.Models.ViewModels;

namespace Backend6.Controllers
{
    public class ForumCategoriesController : Controller
    {
        private readonly ApplicationDbContext context;

        public ForumCategoriesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: ForumCategories
        public async Task<IActionResult> Index()
        {
            return this.View(
                await this.context.ForumCategories
                .Include(x => x.Forums)
                .ThenInclude(c => c.ForumTopics)
                .ToListAsync());
        }

        // GET: ForumCategories/Create
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public IActionResult Create()
        {
            return this.View(new ForumCategoryEditModel());
        }

        // POST: ForumCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumCategoryEditModel model)
        {
            if (ModelState.IsValid)
            {
                var forumCategory = new ForumCategory
                {
                    Name = model.Name
                };

                this.context.ForumCategories.Add(forumCategory);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: ForumCategories/Edit/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this.context.ForumCategories.SingleOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return this.NotFound();
            }

            var model = new ForumCategoryEditModel
            {
                Name = forumCategory.Name
            };

            return this.View(model);
        }

        // POST: ForumCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ForumCategoryEditModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this.context.ForumCategories.SingleOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                forumCategory.Name = model.Name;
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        // GET: ForumCategories/Delete/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return this.NotFound();
            }

            return this.View(forumCategory);
        }

        // POST: ForumCategories/Delete/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forumCategory = await this.context.ForumCategories.SingleOrDefaultAsync(m => m.Id == id);
            this.context.ForumCategories.Remove(forumCategory);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        private bool ForumCategoryExists(Guid id)
        {
            return context.ForumCategories.Any(e => e.Id == id);
        }
    }
}
