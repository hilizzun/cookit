using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBlockingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 52);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "Calories", "Carbohydrates", "Fats", "IsByPiece", "Name", "Proteins" },
                values: new object[,]
                {
                    { 1, 160.0, 8.5, 14.699999999999999, false, "Авокадо", 2.0 },
                    { 2, 89.0, 22.800000000000001, 0.29999999999999999, false, "Бананы", 1.1000000000000001 },
                    { 3, 23.0, 2.7000000000000002, 0.59999999999999998, false, "Базилик", 3.2000000000000002 },
                    { 4, 250.0, 0.0, 15.0, false, "Говядина", 26.0 },
                    { 5, 66.0, 5.7999999999999998, 3.7000000000000002, false, "Горчица", 4.7000000000000002 },
                    { 6, 654.0, 13.699999999999999, 65.200000000000003, false, "Грецкие орехи", 15.199999999999999 },
                    { 7, 22.0, 3.2999999999999998, 0.29999999999999999, false, "Грибы", 3.1000000000000001 },
                    { 8, 105.0, 15.800000000000001, 1.7, false, "Дрожжи", 12.800000000000001 },
                    { 9, 36.0, 6.4000000000000004, 0.40000000000000002, false, "Зелень", 2.6000000000000001 },
                    { 10, 59.0, 4.7000000000000002, 3.2999999999999998, false, "Йогурт", 4.7000000000000002 },
                    { 11, 19.0, 3.1000000000000001, 0.29999999999999999, false, "Кабачки", 0.59999999999999998 },
                    { 12, 77.0, 16.300000000000001, 0.40000000000000002, false, "Картофель", 2.0 },
                    { 13, 51.0, 4.0999999999999996, 1.5, false, "Кефир", 3.3999999999999999 },
                    { 14, 247.0, 80.599999999999994, 1.2, false, "Корица", 4.0 },
                    { 15, 86.0, 19.0, 1.2, false, "Кукуруза", 3.2000000000000002 },
                    { 16, 190.0, 0.0, 12.0, false, "Курица", 20.0 },
                    { 17, 29.0, 9.3000000000000007, 0.29999999999999999, false, "Лимон", 1.1000000000000001 },
                    { 18, 40.0, 9.3000000000000007, 0.10000000000000001, false, "Лук", 1.1000000000000001 },
                    { 19, 131.0, 24.899999999999999, 1.1000000000000001, false, "Макароны", 5.0 },
                    { 20, 884.0, 0.0, 100.0, false, "Масло оливковое", 0.0 },
                    { 21, 717.0, 0.80000000000000004, 81.0, false, "Масло сливочное", 0.5 },
                    { 22, 884.0, 0.0, 100.0, false, "Масло подсолнечное", 0.0 },
                    { 23, 304.0, 82.400000000000006, 0.0, false, "Мед", 0.29999999999999999 },
                    { 24, 60.0, 4.7999999999999998, 3.6000000000000001, false, "Молоко", 3.2000000000000002 },
                    { 25, 41.0, 9.5999999999999996, 0.20000000000000001, false, "Морковь", 0.90000000000000002 },
                    { 26, 364.0, 76.099999999999994, 1.1000000000000001, false, "Мука", 10.300000000000001 },
                    { 27, 15.0, 3.6000000000000001, 0.10000000000000001, false, "Огурцы", 0.69999999999999996 },
                    { 28, 26.0, 4.5999999999999996, 0.29999999999999999, false, "Перец болгарский", 0.90000000000000002 },
                    { 29, 251.0, 64.0, 3.2999999999999998, false, "Перец черный", 10.4 },
                    { 30, 31.0, 5.2000000000000002, 0.80000000000000004, false, "Петрушка", 3.7999999999999998 },
                    { 31, 18.0, 3.8999999999999999, 0.20000000000000001, false, "Помидоры", 0.90000000000000002 },
                    { 32, 130.0, 28.199999999999999, 0.29999999999999999, false, "Рис", 2.7000000000000002 },
                    { 33, 132.0, 0.0, 4.9000000000000004, false, "Рыба", 20.399999999999999 },
                    { 34, 387.0, 99.799999999999997, 0.0, false, "Сахар", 0.0 },
                    { 35, 242.0, 0.0, 19.300000000000001, false, "Свинина", 16.699999999999999 },
                    { 36, 16.0, 3.0, 0.10000000000000001, false, "Сельдерей", 0.69999999999999996 },
                    { 37, 342.0, 3.3999999999999999, 36.0, false, "Сливки", 2.1000000000000001 },
                    { 38, 206.0, 3.3999999999999999, 20.0, false, "Сметана", 2.5 },
                    { 39, 0.0, 0.0, 0.0, false, "Соль", 0.0 },
                    { 40, 360.0, 0.0, 30.0, false, "Сыр", 23.0 },
                    { 41, 155.0, 3.1000000000000001, 8.0, false, "Творог", 18.0 },
                    { 42, 71.0, 11.199999999999999, 0.5, false, "Томатная паста", 5.7000000000000002 },
                    { 43, 18.0, 0.90000000000000002, 0.0, false, "Уксус", 0.10000000000000001 },
                    { 44, 87.0, 13.4, 0.5, false, "Фасоль", 5.2000000000000002 },
                    { 45, 265.0, 49.399999999999999, 3.2000000000000002, false, "Хлеб", 9.0 },
                    { 46, 149.0, 33.100000000000001, 0.5, false, "Чеснок", 6.4000000000000004 },
                    { 47, 546.0, 59.399999999999999, 32.399999999999999, false, "Шоколад", 4.9000000000000004 },
                    { 48, 52.0, 11.4, 0.29999999999999999, false, "Яблоки", 0.40000000000000002 },
                    { 49, 157.0, 0.69999999999999996, 11.5, true, "Яйца", 12.699999999999999 },
                    { 50, 43.0, 9.5999999999999996, 0.40000000000000002, false, "Ягоды", 0.69999999999999996 },
                    { 51, 228.0, 57.899999999999999, 13.699999999999999, false, "Какао-порошок", 19.600000000000001 },
                    { 52, 43.0, 9.5999999999999996, 0.10000000000000001, false, "Свекла", 1.6000000000000001 }
                });
        }
    }
}
