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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend6.Services;

namespace Backend6.Controllers
{
    [Authorize]
    public class ForumMessagesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumMessages/Create
        public async Task<IActionResult> Create(Guid? topicId)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .SingleOrDefaultAsync(m => m.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Topic = topic;
            return this.View(new ForumMessageEditModel());
        }

        // POST: ForumMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? topicId, ForumMessageEditModel model)
        {
            if (topicId == null)
            {
                return this.NotFound();
            }

            var topic = await this.context.ForumTopics
                .SingleOrDefaultAsync(m => m.Id == topicId);

            if (topic == null)
            {
                return this.NotFound();
            }

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            if (this.ModelState.IsValid)
            {
                var now = DateTime.UtcNow;
                var message = new ForumMessage
                {
                    ForumTopicId = topic.Id,
                    CreatorId = user.Id,
                    Created = now,
                    Modified = now,
                    Text = model.Text
                };

                this.context.Add(message);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "ForumTopics", new { topicId = topic.Id });
            }

            this.ViewBag.Topic = topic;
            return this.View(model);
        }

        // GET: ForumMessages/Edit/5
        public async Task<IActionResult> Edit(Guid? messageId)
        {
            if (messageId == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .Include(x => x.ForumTopic)
                .SingleOrDefaultAsync(m => m.Id == messageId);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            var model = new ForumMessageEditModel
            {
                Text = message.Text
            };

            this.ViewBag.Message = message;
            return this.View(model);
        }

        // POST: ForumMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? messageId, ForumMessageEditModel model)
        {
            if (messageId == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .Include(x => x.ForumTopic)
                .SingleOrDefaultAsync(m => m.Id == messageId);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                message.Text = model.Text;
                message.Modified = DateTime.UtcNow;

                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index", "ForumTopics", new { topicId = message.ForumTopicId });
            }

            this.ViewBag.Message = message;
            return this.View(model);
        }

        // GET: ForumMessages/Delete/5
        public async Task<IActionResult> Delete(Guid? messageId)
        {
            if (messageId == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .Include(p => p.ForumTopic)
                .Include(p => p.Creator)
                .SingleOrDefaultAsync(m => m.Id == messageId);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            return this.View(message);
        }

        // POST: ForumMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid messageId)
        {
            if (messageId == null)
            {
                return this.NotFound();
            }

            var message = await this.context.ForumMessages
                .Include(p => p.Creator)
                .Include(p => p.ForumTopic)
                .SingleOrDefaultAsync(m => m.Id == messageId);

            if (message == null || !this.userPermissions.CanEditForumMessage(message))
            {
                return this.NotFound();
            }

            this.context.ForumMessages.Remove(message);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index", "ForumTopics", new { topicId = message.ForumTopicId });
        }

    }
}
