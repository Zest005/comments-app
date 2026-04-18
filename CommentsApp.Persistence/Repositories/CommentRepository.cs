using CommentsApp.Application.Common.Interfaces;
using CommentsApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Persistence.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        public readonly ApplicationDbContext _context;

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
                await _context.Comments
                    .Where(c => c.ParentCommentId != null)
                    .Include(c => c.Attachment)
                    .AsSplitQuery()
                    .LoadAsync();
            }

            return (comments, totalCount);
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
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

        public async Task<int> GetRootCommentsCountAsync()
        {
            return await _context.Comments.CountAsync(c => c.ParentCommentId == null);
        }
    }
}
