using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Backend6.Models.ViewModels;
using Backend6.Services;

namespace Backend6.Controllers
{
    public class ForumTopicsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumTopicsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumTopics
        public async Task<IActionResult> Index(Guid? topicId)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
               .Include(x => x.Creator)
               .Include(x => x.Forum)
               .Include(x => x.ForumMessages)
               .ThenInclude(c => c.Attachments)
               .Include(x => x.ForumMessages)
               .ThenInclude(x => x.Creator)
               .SingleOrDefaultAsync(m => m.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            return this.View(topic);
        }

        // GET: ForumTopics/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumTopic == null)
            {
                return NotFound();
            }

            return View(forumTopic);
        }

        // GET: ForumTopics/Create
        [Authorize]
        public async Task<IActionResult> Create(Guid? forumId)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .SingleOrDefaultAsync(m => m.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Forum = forum;
            return this.View(new ForumTopicEditModel());
        }

        // POST: ForumTopics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? forumId, ForumTopicEditModel model)
        {
            if (forumId == null)
            {
                return this.NotFound();
            }

            var forum = await this.context.Forums
                .SingleOrDefaultAsync(m => m.Id == forumId);

            if (forum == null)
            {
                return this.NotFound();
            }

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);
            if (this.ModelState.IsValid)
            {
                var now = DateTime.UtcNow;
                var forumTopic = new ForumTopic
                {
                    Name = model.Name,
                    CreatorId = user.Id,
                    Created = now,
                    ForumId = forum.Id
                };

                this.context.Add(forumTopic);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "Forums", new { forumId = forum.Id});
                //return this.RedirectToAction("Details", "Forums", new { id = forumCategory.Id });
            }

            this.ViewBag.Forum = forum;
            return this.View(model);
        }

        // GET: ForumTopics/Edit/5
        public async Task<IActionResult> Edit(Guid? topicId)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics.SingleOrDefaultAsync(m => m.Id == topicId);
            if (topic == null || !this.userPermissions.CanEditTopic(topic))
            {
                return this.NotFound();
            }

            var model = new ForumTopicEditModel
            {
                Name = topic.Name
            };

            this.ViewBag.Topic = topic;
            return this.View(model);
        }

        // POST: ForumTopics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid topicId, ForumTopicEditModel model)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics.SingleOrDefaultAsync(m => m.Id == topicId);
            if (topic == null || !this.userPermissions.CanEditTopic(topic))
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                topic.Name = model.Name;

                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "Forums", new { forumId = topic.ForumId });
            }

            this.ViewBag.Topic = topic;
            return this.View(model);
        }

        // GET: ForumTopics/Delete/5
        public async Task<IActionResult> Delete(Guid? topicId)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var forumTopic = await context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .SingleOrDefaultAsync(m => m.Id == topicId);

            if (forumTopic == null || !this.userPermissions.CanEditTopic(forumTopic))
            {
                return this.NotFound();
            }

            return this.View(forumTopic);
        }

        // POST: ForumTopics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid topicId)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var forumTopic = await this.context.ForumTopics.SingleOrDefaultAsync(m => m.Id == topicId);

            if (forumTopic == null || !this.userPermissions.CanEditTopic(forumTopic))
            {
                return this.NotFound();
            }

            this.context.ForumTopics.Remove(forumTopic);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index", "Forums", new { forumId = forumTopic.ForumId });
        }

        private bool ForumTopicExists(Guid id)
        {
            return context.ForumTopics.Any(e => e.Id == id);
        }
    }
}
