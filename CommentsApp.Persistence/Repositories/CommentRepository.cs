using CommentsApp.Application.Common.Interfaces;
using CommentsApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommentsApp.Persistence.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Comment> Comments, int TotalCount)> GetRootCommentsAsync(
            int page,
            int pageSize,
            string sortBy,
            bool sortDescending)
        {
            var query = _context.Comments
                .Where(c => c.ParentCommentId == null)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            query = sortBy.ToLower() switch
            {
                "username" => sortDescending ? query.OrderByDescending(c => c.UserName) : query.OrderBy(c => c.UserName),
                "email" => sortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
                _ => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
            };

            var rootComments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Attachment)
                .AsSplitQuery()
                .ToListAsync();

            if (rootComments.Count > 0)
            {
                var rootIds = rootComments.Select(c => c.Id).ToList();
                await LoadRepliesRecursiveAsync(rootIds);
            }

            return (rootComments, totalCount);
        }

        private async Task LoadRepliesRecursiveAsync(List<int> parentIds)
        {
            var children = await _context.Comments
                .Where(c => c.ParentCommentId != null && parentIds.Contains(c.ParentCommentId.Value))
                .Include(c => c.Attachment)
                .AsSplitQuery()
                .ToListAsync();

            if (children.Count > 0)
            {
                var childIds = children.Select(c => c.Id).ToList();
                await LoadRepliesRecursiveAsync(childIds);
            }
        }

        public Task<Comment?> GetByIdAsync(int id)
        {
            return _context.Comments
                .Include(c => c.Attachment)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Attachment)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return comment;
        }

        public Task<int> GetRootCommentsCountAsync()
        {
            return _context.Comments.CountAsync(c => c.ParentCommentId == null);
        }
    }
}
