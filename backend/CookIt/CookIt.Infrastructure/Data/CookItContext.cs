using CookIt.Core.Entities;
using CookIt.Infrastructure.Configuration.EntityFramework;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CookItContext).Assembly);
            DatabaseSeeder.Seed(modelBuilder);
        }
    }
}