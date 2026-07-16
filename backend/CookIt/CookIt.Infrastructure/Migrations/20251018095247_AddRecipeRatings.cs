using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Recipe_DifficultyLevel_Range",
                table: "Recipes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Recipe_SpicinessLevel_Range",
                table: "Recipes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recipe_DifficultyLevel_Range",
                table: "Recipes",
                sql: "\"DifficultyLevel\" >= 1 AND \"DifficultyLevel\" <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recipe_SpicinessLevel_Range",
                table: "Recipes",
                sql: "\"SpicinessLevel\" >= 1 AND \"SpicinessLevel\" <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Recipe_DifficultyLevel_Range",
                table: "Recipes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Recipe_SpicinessLevel_Range",
                table: "Recipes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recipe_DifficultyLevel_Range",
                table: "Recipes",
                sql: "\"DifficultyLevel\" >= 0 AND \"DifficultyLevel\" <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recipe_SpicinessLevel_Range",
                table: "Recipes",
                sql: "\"SpicinessLevel\" >= 0 AND \"SpicinessLevel\" <= 5");
        }
    }
}
