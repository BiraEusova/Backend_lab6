using System;
using Backend6.Models;

namespace Backend6.Services
{
    public interface IUserPermissionsService
    {
        Boolean CanEditPost(Post post);

        Boolean CanEditPostComment(PostComment postComment);

        Boolean CanEditTopic(ForumTopic forumTopic);

        Boolean CanEditForumMessage(ForumMessage forumMessage);
    }
}