using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addNutritionInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_Unit_UnitId",
                table: "RecipeIngredients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Unit",
                table: "Unit");

            migrationBuilder.RenameTable(
                name: "Unit",
                newName: "Units");

            migrationBuilder.AddColumn<double>(
                name: "Calories",
                table: "Ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Carbohydrates",
                table: "Ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Fats",
                table: "Ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsByPiece",
                table: "Ingredients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Proteins",
                table: "Ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Units",
                table: "Units",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b8633e2d-a33b-45e6-8329-1958b3252bbd",
                column: "NormalizedName",
                value: "MODERATOR");

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 364.0, 76.099999999999994, 1.1000000000000001, false, 10.300000000000001 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 387.0, 99.799999999999997, 0.0, false, 0.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 0.0, 0.0, 0.0, false, 0.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 157.0, 0.69999999999999996, 11.5, true, 12.699999999999999 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 60.0, 4.7999999999999998, 3.6000000000000001, false, 3.2000000000000002 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 717.0, 0.80000000000000004, 81.0, false, 0.5 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 884.0, 0.0, 100.0, false, 0.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 77.0, 16.300000000000001, 0.40000000000000002, false, 2.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 40.0, 9.3000000000000007, 0.10000000000000001, false, 1.1000000000000001 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 149.0, 33.100000000000001, 0.5, false, 6.4000000000000004 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 41.0, 9.5999999999999996, 0.20000000000000001, false, 0.90000000000000002 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 190.0, 0.0, 12.0, false, 20.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 250.0, 0.0, 15.0, false, 26.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 130.0, 28.199999999999999, 0.29999999999999999, false, 2.7000000000000002 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 18.0, 3.8999999999999999, 0.20000000000000001, false, 0.90000000000000002 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 15.0, 3.6000000000000001, 0.10000000000000001, false, 0.69999999999999996 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 360.0, 0.0, 30.0, false, 23.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 206.0, 3.3999999999999999, 20.0, false, 2.5 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 624.0, 0.59999999999999998, 70.0, false, 0.29999999999999999 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Calories", "Carbohydrates", "Fats", "IsByPiece", "Proteins" },
                values: new object[] { 36.0, 6.4000000000000004, 0.40000000000000002, false, 2.6000000000000001 });

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_Units_UnitId",
                table: "RecipeIngredients",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_Units_UnitId",
                table: "RecipeIngredients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Units",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Calories",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Carbohydrates",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Fats",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "IsByPiece",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Proteins",
                table: "Ingredients");

            migrationBuilder.RenameTable(
                name: "Units",
                newName: "Unit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Unit",
                table: "Unit",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b8633e2d-a33b-45e6-8329-1958b3252bbd",
                column: "NormalizedName",
                value: "Moderator");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_Unit_UnitId",
                table: "RecipeIngredients",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id");
        }
    }
}
