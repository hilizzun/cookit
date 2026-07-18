using CookIt.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookIt.Infrastructure.Configuration.EntityFramework
{
    internal class UserFavoriteConfiguration : IEntityTypeConfiguration<UserFavorite>
    {
        public void Configure(EntityTypeBuilder<UserFavorite> builder)
        {
            builder.HasKey(userFavorite => new
            {
                userFavorite.UserId,
                userFavorite.RecipeId
            });

            builder.HasOne(userFavorite => userFavorite.User)
                .WithMany(user => user.Favorites)
                .HasForeignKey(userFavorite => userFavorite.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(userFavorite => userFavorite.Recipe)
                .WithMany(recipe => recipe.FavoritedBy)
                .HasForeignKey(userFavorite => userFavorite.RecipeId);
        }
    }
}