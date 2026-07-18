using CookIt.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookIt.Infrastructure.Configuration.EntityFramework
{
    internal class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.FullDescription)
                .IsRequired();

            builder.Property(r => r.ShortDescription)
                .IsRequired();

            builder.Property(r => r.CookingTimeWithUser)
                .IsRequired();

            builder.Property(r => r.CookingTimeWithoutUser)
                .IsRequired();

            builder.Property(r => r.SpicinessLevel)
                .IsRequired();

            builder.Property(r => r.DifficultyLevel)
                .IsRequired();

            builder.HasOne(r => r.DishType)
                .WithMany()
                .HasForeignKey(r => r.DishTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.Name, r.ShortDescription, r.FullDescription })
                .HasMethod("GIN")
                .IsTsVectorExpressionIndex("russian");

            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Recipe_SpicinessLevel_Range", "\"SpicinessLevel\" >= 1 AND \"SpicinessLevel\" <= 5");
                tb.HasCheckConstraint("CK_Recipe_DifficultyLevel_Range", "\"DifficultyLevel\" >= 1 AND \"DifficultyLevel\" <= 5");
                tb.HasCheckConstraint("CK_Recipe_CookingTimeWithUser_Range", "\"CookingTimeWithUser\" >= 1");
                tb.HasCheckConstraint("CK_Recipe_CookingTimeWithoutUser_Range", "\"CookingTimeWithoutUser\" >= 1");
            });
        }
    }

}
