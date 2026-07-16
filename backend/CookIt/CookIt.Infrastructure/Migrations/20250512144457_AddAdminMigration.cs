using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2c5e174e-3b0e-446f-86af-483d56fd7210", null, "User", "USER" },
                    { "8e445865-a24d-4543-a6c6-9443d048cdb9", null, "Admin", "ADMIN" },
                    { "b8633e2d-a33b-45e6-8329-1958b3252bbd", null, "Moderator", "Moderator" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiryTime", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "b8f6982f-8c8a-40cf-8350-6fccb723cf34", 0, "59a80b94-5765-4bf9-ac42-2cca148f6eb2", "admin@cookit.com", true, "admin", true, null, "ADMIN@COOKIT.COM", "ADMIN", "AQAAAAIAAYagAAAAECJp7LhsC6Be7WPS0mPtlJt9Px5Eje2mpCAH+DXXNCr/PfJE2aH3blhK6fpym4YQMA==", null, true, null, null, "4G4YRSV6FEXCB6A7DC2YXLSDI5Q3XXFJ", false, "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "8e445865-a24d-4543-a6c6-9443d048cdb9", "b8f6982f-8c8a-40cf-8350-6fccb723cf34" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b8633e2d-a33b-45e6-8329-1958b3252bbd");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8e445865-a24d-4543-a6c6-9443d048cdb9", "b8f6982f-8c8a-40cf-8350-6fccb723cf34" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b8f6982f-8c8a-40cf-8350-6fccb723cf34");
        }
    }
}
