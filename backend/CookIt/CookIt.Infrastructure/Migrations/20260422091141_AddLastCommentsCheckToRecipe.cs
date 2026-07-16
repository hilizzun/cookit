using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastCommentsCheckToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCommentsCheck",
                table: "Recipes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "4cea2eb3-79bc-4959-b892-b38fadd53ef0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                column: "ConcurrencyStamp",
                value: "d4d97ef1-fb14-4f83-be4e-b6a38d506c3c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b8633e2d-a33b-45e6-8329-1958b3252bbd",
                column: "ConcurrencyStamp",
                value: "656a237b-33d4-481c-bf2e-2304c9cc7ab7");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCommentsCheck",
                table: "Recipes");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "fb5d3cc4-4825-4f4e-b372-888603920492");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                column: "ConcurrencyStamp",
                value: "8475c17d-b6de-4d82-bc3b-9e3ed7330d40");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b8633e2d-a33b-45e6-8329-1958b3252bbd",
                column: "ConcurrencyStamp",
                value: "2c1f746f-60cd-499d-a90c-16baba0f433a");
        }
    }
}
