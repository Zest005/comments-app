using CommentsApp.Domain.Entities;

namespace CommentsApp.Application.Common.Interfaces
{
    public interface ICommentRepository
    {
        Task<(List<Comment> Comments, int TotalCount)> GetRootCommentsAsync(
            int page,
            int pageSize,
            string sortBy,
            bool sortDescending);

        Task<Comment?> GetByIdAsync(int id);

        Task<Comment> AddAsync(Comment comment);

        Task<int> GetRootCommentsCountAsync();

        Task<(List<Comment> Replies, int TotalCount)> GetRepliesAsync(int parentId, int skip, int take);
    }
}
