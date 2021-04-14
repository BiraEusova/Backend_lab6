using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Backend6.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Backend6.Services;
using Microsoft.AspNetCore.Authorization;

namespace Backend6.Controllers
{
    public class ForumsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: Forum
        public async Task<IActionResult> Index(Guid? forumId)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .Include(h => h.ForumTopics)
                .ThenInclude(h => h.ForumMessages)
                .ThenInclude(h => h.Creator)
                .Include(h => h.ForumTopics)
                .ThenInclude(h => h.Creator)
                .SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            var lastMessage = await this.context.Forums
                .Include(h => h.ForumTopics)
                .ThenInclude(h => h.ForumMessages)
                .ThenInclude(h => h.Creator)
                .SingleOrDefaultAsync(x => x.Id == forumId);

            this.ViewData["LastMessage"] = lastMessage;
            this.ViewBag.Forum = forum;
            return this.View(forum);
        }


        // GET: Fora/Create
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public async Task<IActionResult> Create(Guid? forumCategoryId)
        {
            if (forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == forumCategoryId);
            if (forumCategory == null)
            {
                return this.NotFound();
            }

            this.ViewBag.ForumCategory = forumCategory;
            return this.View(new ForumEditModel());
        }

        // POST: Fora/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? forumCategoryId, ForumEditModel model)
        {
            if (forumCategoryId == null)
            {
                return this.NotFound();
            }

            var forumCategory = await this.context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == forumCategoryId);
            if (forumCategory == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                var forum = new Forum
                {
                    Name = model.Name,
                    Description = model.Description,
                    ForumCategoryId = forumCategory.Id
                };

                this.context.Add(forum);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "ForumCategories");
                //return this.RedirectToAction("Details", "Forums", new { id = forumCategory.Id });
            }

            this.ViewBag.ForumCategory = forumCategory;
            return this.View(model);
        }

        // GET: Forum/Edit/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public async Task<IActionResult> Edit(Guid? forumId)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums.SingleOrDefaultAsync(m => m.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            var model = new ForumEditModel
            {
                Name = forum.Name,
                Description = forum.Description
            };

            this.ViewBag.Forum = forum;
            return this.View(model);
        }

        // POST: Forum/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid forumId, ForumEditModel model)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums.SingleOrDefaultAsync(m => m.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                forum.Name = model.Name;
                forum.Description = model.Description;
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "ForumCategories");
            }

            this.ViewBag.Forum = forum;
            return this.View(model);
        }

        // GET: Fora/Delete/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public async Task<IActionResult> Delete(Guid? forumId)
        {
            if (forumId == null)
            {
                return NotFound();
            }

            var forum = await this.context.Forums
                .Include(f => f.ForumCategory)
                .SingleOrDefaultAsync(m => m.Id == forumId);

            if (forum == null)
            {
                return NotFound();
            }

            return this.View(forum);
        }

        // POST: Fora/Delete/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid forumId)
        {
            var forum = await this.context.Forums.SingleOrDefaultAsync(m => m.Id == forumId);
            this.context.Forums.Remove(forum);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index", "ForumCategories");
        }

        private bool ForumExists(Guid id)
        {
            return context.Forums.Any(e => e.Id == id);
        }
    }
}
