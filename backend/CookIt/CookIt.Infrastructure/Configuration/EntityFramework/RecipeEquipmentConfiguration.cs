using CookIt.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookIt.Infrastructure.Configuration.EntityFramework
{
    internal class RecipeEquipmentConfiguration : IEntityTypeConfiguration<RecipeEquipment>
    {
        public void Configure(EntityTypeBuilder<RecipeEquipment> builder)
        {
            builder.HasKey(recipeEquipment => new
            {
                recipeEquipment.RecipeId,
                recipeEquipment.EquipmentId
            });

            builder.HasOne(recipeEquipment => recipeEquipment.Recipe)
                .WithMany(recipe => recipe.RecipeEquipments)
                .HasForeignKey(recipeEquipment => recipeEquipment.RecipeId);

            builder.HasOne(recipeEquipment => recipeEquipment.Equipment)
                .WithMany(equipment => equipment.RecipeEquipments)
                .HasForeignKey(recipeEquipment => recipeEquipment.EquipmentId);
        }
    }
}