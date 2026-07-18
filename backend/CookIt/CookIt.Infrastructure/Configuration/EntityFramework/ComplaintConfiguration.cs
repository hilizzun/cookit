using CookIt.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookIt.Infrastructure.Configuration.EntityFramework
{
    internal class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
    {
        public void Configure(EntityTypeBuilder<Complaint> builder)
        {
            builder.HasOne(complaint => complaint.Comment)
                .WithMany()
                .HasForeignKey(complaint => complaint.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(complaint => complaint.User)
                .WithMany()
                .HasForeignKey(complaint => complaint.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(complaint => complaint.ResolvedBy)
                .WithMany()
                .HasForeignKey(complaint => complaint.ResolvedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}