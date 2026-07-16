using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookIt.Infrastructure.Migrations
{
    public partial class AddIsDeletedFieldsToReferenceTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Для Ingredients
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Ingredients' AND column_name = 'IsDeleted'
                    ) THEN
                        ALTER TABLE ""Ingredients"" ADD COLUMN ""IsDeleted"" boolean NOT NULL DEFAULT false;
                    END IF;
                    
                    -- Для Units
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Units' AND column_name = 'IsDeleted'
                    ) THEN
                        ALTER TABLE ""Units"" ADD COLUMN ""IsDeleted"" boolean NOT NULL DEFAULT false;
                    END IF;
                    
                    -- Для DishTypes
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'DishTypes' AND column_name = 'IsDeleted'
                    ) THEN
                        ALTER TABLE ""DishTypes"" ADD COLUMN ""IsDeleted"" boolean NOT NULL DEFAULT false;
                    END IF;
                    
                    -- Для Equipments
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Equipments' AND column_name = 'IsDeleted'
                    ) THEN
                        ALTER TABLE ""Equipments"" ADD COLUMN ""IsDeleted"" boolean NOT NULL DEFAULT false;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""DishTypes"" SET ""IsDeleted"" = false WHERE ""IsDeleted"" IS NULL;
                UPDATE ""Equipments"" SET ""IsDeleted"" = false WHERE ""IsDeleted"" IS NULL;
                UPDATE ""Ingredients"" SET ""IsDeleted"" = false WHERE ""IsDeleted"" IS NULL;
                UPDATE ""Units"" SET ""IsDeleted"" = false WHERE ""IsDeleted"" IS NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Ingredients"" DROP COLUMN IF EXISTS ""IsDeleted"";
                ALTER TABLE ""Units"" DROP COLUMN IF EXISTS ""IsDeleted"";
                ALTER TABLE ""DishTypes"" DROP COLUMN IF EXISTS ""IsDeleted"";
                ALTER TABLE ""Equipments"" DROP COLUMN IF EXISTS ""IsDeleted"";
            ");
        }
    }
}