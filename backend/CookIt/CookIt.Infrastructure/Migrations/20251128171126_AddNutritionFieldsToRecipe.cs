using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionFieldsToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CaloriesPer100g",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CaloriesPerServing",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CarbohydratesPer100g",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CarbohydratesPerServing",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FatsPer100g",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FatsPerServing",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ProteinsPer100g",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ProteinsPerServing",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalCalories",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalCarbohydrates",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalFats",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalProteins",
                table: "Recipes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaloriesPer100g",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CaloriesPerServing",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CarbohydratesPer100g",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CarbohydratesPerServing",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "FatsPer100g",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "FatsPerServing",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ProteinsPer100g",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ProteinsPerServing",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TotalCalories",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TotalCarbohydrates",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TotalFats",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TotalProteins",
                table: "Recipes");
        }
    }
}
