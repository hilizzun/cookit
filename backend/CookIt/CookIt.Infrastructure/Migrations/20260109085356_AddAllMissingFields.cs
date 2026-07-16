using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавляем поля блокировки пользователя
            migrationBuilder.Sql(@"
        DO $$
        BEGIN
            -- IsBlocked
            IF NOT EXISTS (
                SELECT 1 FROM information_schema.columns 
                WHERE table_name = 'AspNetUsers' AND column_name = 'IsBlocked'
            ) THEN
                ALTER TABLE ""AspNetUsers"" ADD COLUMN ""IsBlocked"" boolean NOT NULL DEFAULT false;
            END IF;
            
            -- BlockedReason
            IF NOT EXISTS (
                SELECT 1 FROM information_schema.columns 
                WHERE table_name = 'AspNetUsers' AND column_name = 'BlockedReason'
            ) THEN
                ALTER TABLE ""AspNetUsers"" ADD COLUMN ""BlockedReason"" text;
            END IF;
            
            -- BlockedUntil
            IF NOT EXISTS (
                SELECT 1 FROM information_schema.columns 
                WHERE table_name = 'AspNetUsers' AND column_name = 'BlockedUntil'
            ) THEN
                ALTER TABLE ""AspNetUsers"" ADD COLUMN ""BlockedUntil"" timestamp with time zone;
            END IF;
        END $$;
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""IsBlocked"";
        ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""BlockedReason"";
        ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""BlockedUntil"";
    ");
        }
    }
}
