using CookIt.Core.Entities;
using CookIt.Infrastructure.Configuration.EntityFramework;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CookIt.Infrastructure
{
    public class CookItContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<DishType> DishTypes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<RecipeEquipment> RecipeEquipments { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<RecipeRating> RecipeRatings { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<InterestingFact> InterestingFacts { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListExcludedIngredient> ShoppingListExcludedIngredients { get; set; }
        public DbSet<UserStatistics> UserStatistics { get; set; }

        public CookItContext(DbContextOptions<CookItContext> options)
            : base(options)
        {
            Database.CanConnect();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RecipeConfiguration());
            modelBuilder.ApplyConfiguration(new DishTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EquipmentConfiguration());
            modelBuilder.ApplyConfiguration(new IngredientConfiguration());

            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);

            modelBuilder.Entity<RecipeEquipment>()
                .HasKey(re => new { re.RecipeId, re.EquipmentId });
            modelBuilder.Entity<RecipeEquipment>()
                .HasOne(re => re.Recipe)
                .WithMany(r => r.RecipeEquipments)
                .HasForeignKey(re => re.RecipeId);
            modelBuilder.Entity<RecipeEquipment>()
                .HasOne(re => re.Equipment)
                .WithMany(e => e.RecipeEquipments)
                .HasForeignKey(re => re.EquipmentId);

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(uf => new { uf.UserId, uf.RecipeId });

                entity.HasOne(uf => uf.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(uf => uf.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uf => uf.Recipe)
                    .WithMany(r => r.FavoritedBy)
                    .HasForeignKey(uf => uf.RecipeId);
            });

            modelBuilder.Entity<RecipeRating>(entity =>
            {
                entity.HasKey(rr => new { rr.UserId, rr.RecipeId });

                entity.HasOne(rr => rr.User)
                    .WithMany(u => u.Ratings)
                    .HasForeignKey(rr => rr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rr => rr.Recipe)
                    .WithMany(r => r.Ratings)
                    .HasForeignKey(rr => rr.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasCheckConstraint("CK_RecipeRating_Value_Range", "\"Value\" >= 1 AND \"Value\" <= 5");
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasIndex(r => new { r.Name, r.ShortDescription, r.FullDescription })
                    .HasMethod("GIN")
                    .IsTsVectorExpressionIndex("russian");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Recipe)
                    .WithMany()
                    .HasForeignKey(c => c.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Complaint>(entity =>
            {
                entity.HasOne(c => c.Comment)
                    .WithMany()
                    .HasForeignKey(c => c.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ResolvedBy)
                    .WithMany()
                    .HasForeignKey(c => c.ResolvedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            DatabaseSeeder.Seed(modelBuilder);
        }
    }
}
