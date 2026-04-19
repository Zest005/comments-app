using CommentsApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommentsApp.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<CommentAttachment> CommentAttachments => Set<CommentAttachment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
