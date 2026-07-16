using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DishTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DishTypeId = table.Column<int>(type: "integer", nullable: false),
                    CookingTimeWithUser = table.Column<int>(type: "integer", nullable: false),
                    CookingTimeWithoutUser = table.Column<int>(type: "integer", nullable: false),
                    SpicinessLevel = table.Column<int>(type: "integer", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.CheckConstraint("CK_Recipe_DifficultyLevel_Range", "\"DifficultyLevel\" >= 0 AND \"DifficultyLevel\" <= 5");
                    table.CheckConstraint("CK_Recipe_SpicinessLevel_Range", "\"SpicinessLevel\" >= 0 AND \"SpicinessLevel\" <= 5");
                    table.ForeignKey(
                        name: "FK_Recipes_DishTypes_DishTypeId",
                        column: x => x.DishTypeId,
                        principalTable: "DishTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeEquipments",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeEquipments", x => new { x.RecipeId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_RecipeEquipments_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeEquipments_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    IngredientId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => new { x.RecipeId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DishTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Напиток" },
                    { 2, "Основное блюдо" },
                    { 3, "Гарнир" },
                    { 4, "Закуска" },
                    { 5, "Салат" },
                    { 6, "Десерт" },
                    { 7, "Суп" }
                });

            migrationBuilder.InsertData(
                table: "Equipments",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Духовка" },
                    { 2, "Плита" },
                    { 3, "Миксер" },
                    { 4, "Блендер" },
                    { 5, "Мультиварка" },
                    { 6, "Микроволновка" },
                    { 7, "Кастрюля" },
                    { 8, "Сковорода" },
                    { 9, "Венчик" },
                    { 10, "Мерный стакан" },
                    { 11, "Форма для выпечки" }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Мука" },
                    { 2, "Сахар" },
                    { 3, "Соль" },
                    { 4, "Яйцо" },
                    { 5, "Молоко" },
                    { 6, "Масло сливочное" },
                    { 7, "Подсолнечное масло" },
                    { 8, "Картофель" },
                    { 9, "Лук" },
                    { 10, "Чеснок" },
                    { 11, "Морковь" },
                    { 12, "Курица" },
                    { 13, "Говядина" },
                    { 14, "Рис" },
                    { 15, "Помидоры" },
                    { 16, "Огурцы" },
                    { 17, "Сыр" },
                    { 18, "Сметана" },
                    { 19, "Майонез" },
                    { 20, "Зелень" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEquipments_EquipmentId",
                table: "RecipeEquipments",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_IngredientId",
                table: "RecipeIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_DishTypeId",
                table: "Recipes",
                column: "DishTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeEquipments");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "DishTypes");
        }
    }
}
