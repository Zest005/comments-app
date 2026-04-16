using CommentsApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Persistence.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.UserName)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.HomePage)
                .IsRequired(false)
                .HasMaxLength(2048);

            builder.Property(c => c.Text)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();


            builder.HasMany(c => c.Replies)
                .WithOne(c => c.ParentComment)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.Attachment)
                .WithOne(a => a.Comment)
                .HasForeignKey<CommentAttachment>(a => a.CommentId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_Comments_CreatedAt");

            builder.HasIndex(c => c.Email)
                .HasDatabaseName("IX_Comments_Email");

            builder.HasIndex(c => c.UserName)
                .HasDatabaseName("IX_Comments_UserName");

            builder.HasIndex(c => c.ParentCommentId)
                .HasDatabaseName("IX_Comments_ParentCommentId");
        }
    }
}
