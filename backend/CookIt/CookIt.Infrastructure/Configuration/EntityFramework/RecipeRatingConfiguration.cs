using CookIt.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookIt.Infrastructure.Configuration.EntityFramework
{
    internal class RecipeRatingConfiguration : IEntityTypeConfiguration<RecipeRating>
    {
        public void Configure(EntityTypeBuilder<RecipeRating> builder)
        {
            builder.HasKey(recipeRating => new
            {
                recipeRating.UserId,
                recipeRating.RecipeId
            });

            builder.HasOne(recipeRating => recipeRating.User)
                .WithMany(user => user.Ratings)
                .HasForeignKey(recipeRating => recipeRating.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(recipeRating => recipeRating.Recipe)
                .WithMany(recipe => recipe.Ratings)
                .HasForeignKey(recipeRating => recipeRating.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_RecipeRating_Value_Range",
                    "\"Value\" >= 1 AND \"Value\" <= 5"));
        }
    }
}