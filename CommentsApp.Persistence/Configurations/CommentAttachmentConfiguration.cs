using CommentsApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Persistence.Configurations
{
    public class CommentAttachmentConfiguration : IEntityTypeConfiguration<CommentAttachment>
    {
        public void Configure(EntityTypeBuilder<CommentAttachment> builder)
        {
            builder.ToTable("CommentAttachments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.StoredFilePath)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.FileSize)
                .IsRequired();
        }
    }
}
