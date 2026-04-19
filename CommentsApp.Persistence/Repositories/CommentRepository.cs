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

            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Attachment)
                .AsSplitQuery()
                .ToListAsync();

            if (comments.Count > 0)
            {
                var rootIds = comments.Select(c => c.Id).ToList();
                var replyCounts = await _context.Comments
                    .Where(c => c.ParentCommentId != null && rootIds.Contains(c.ParentCommentId.Value))
                    .GroupBy(c => c.ParentCommentId)
                    .Select(g => new { ParentId = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var rc in replyCounts)
                {
                    var parent = comments.FirstOrDefault(c => c.Id == rc.ParentId);
                    if (parent != null)
                    {
                        parent.Replies = Enumerable.Range(0, rc.Count)
                            .Select(_ => new Comment())
                            .ToList();
                    }
                }
            }

            return (comments, totalCount);
        }

        public async Task<(List<Comment> Replies, int TotalCount)> GetRepliesAsync(
            int parentId, int skip, int take)
        {
            var query = _context.Comments
                .Where(c => c.ParentCommentId == parentId)
                .OrderBy(c => c.CreatedAt);

            var totalCount = await query.CountAsync();

            var replies = await query
                .Skip(skip)
                .Take(take)
                .Include(c => c.Attachment)
                .AsSplitQuery()
                .ToListAsync();

            if (replies.Count > 0)
            {
                var replyIds = replies.Select(c => c.Id).ToList();
                var childCounts = await _context.Comments
                    .Where(c => c.ParentCommentId != null && replyIds.Contains(c.ParentCommentId.Value))
                    .GroupBy(c => c.ParentCommentId)
                    .Select(g => new { ParentId = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var cc in childCounts)
                {
                    var reply = replies.FirstOrDefault(c => c.Id == cc.ParentId);
                    if (reply != null)
                    {
                        reply.Replies = Enumerable.Range(0, cc.Count)
                            .Select(_ => new Comment())
                            .ToList();
                    }
                }
            }

            return (replies, totalCount);
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
