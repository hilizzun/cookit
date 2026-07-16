using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeModerationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Recipes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Recipes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Recipes",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionComment",
                table: "Recipes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ApprovedById",
                table: "Recipes",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_AspNetUsers_ApprovedById",
                table: "Recipes",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_AspNetUsers_ApprovedById",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_ApprovedById",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "RejectionComment",
                table: "Recipes");
        }
    }
}
