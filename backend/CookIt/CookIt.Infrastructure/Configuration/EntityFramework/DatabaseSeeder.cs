using CookIt.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CookIt.Infrastructure.Configuration.EntityFramework
{
    internal class DatabaseSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Equipment>().HasData(
                new Equipment { Id = 1, Name = "Духовка" },
                new Equipment { Id = 2, Name = "Плита" },
                new Equipment { Id = 3, Name = "Миксер" },
                new Equipment { Id = 4, Name = "Блендер" },
                new Equipment { Id = 5, Name = "Мультиварка" },
                new Equipment { Id = 6, Name = "Микроволновка" },
                new Equipment { Id = 7, Name = "Кастрюля" },
                new Equipment { Id = 8, Name = "Сковорода" },
                new Equipment { Id = 9, Name = "Венчик" },
                new Equipment { Id = 10, Name = "Мерный стакан" },
                new Equipment { Id = 11, Name = "Форма для выпечки" }
            );

            modelBuilder.Entity<DishType>().HasData(
                new DishType { Id = 1, Name = "Напиток" },
                new DishType { Id = 2, Name = "Основное блюдо" },
                new DishType { Id = 3, Name = "Гарнир" },
                new DishType { Id = 4, Name = "Закуска" },
                new DishType { Id = 5, Name = "Салат" },
                new DishType { Id = 6, Name = "Десерт" },
                new DishType { Id = 7, Name = "Суп" }
            );

            modelBuilder.Entity<Unit>().HasData(
                new Unit { Id = 1, Name = "г", ConversionToGrams = 1.0 },
                new Unit { Id = 2, Name = "кг", ConversionToGrams = 1000.0 },
                new Unit { Id = 3, Name = "мл", ConversionToGrams = 1.0 },
                new Unit { Id = 4, Name = "л", ConversionToGrams = 1000.0 },
                new Unit { Id = 5, Name = "шт", ConversionToGrams = null },
                new Unit { Id = 6, Name = "ч.л.", ConversionToGrams = 5.0 },
                new Unit { Id = 7, Name = "ст.л.", ConversionToGrams = 15.0 },
                new Unit { Id = 8, Name = "стакан", ConversionToGrams = 250.0 },
                new Unit { Id = 9, Name = "щепотка", ConversionToGrams = null },
                new Unit { Id = 10, Name = "по вкусу", ConversionToGrams = null }
            );

            //Seeding a 'Administrator' role to AspNetRoles table
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole()
                {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    Name = "User",
                    NormalizedName = "USER"
                },
                new IdentityRole()
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb9",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole()
                {
                    Id = "b8633e2d-a33b-45e6-8329-1958b3252bbd",
                    Name = "Moderator",
                    NormalizedName = "MODERATOR"
                });

            modelBuilder.Entity<ApplicationUser>().HasData(
               new ApplicationUser()
               {
                   Id = "b8f6982f-8c8a-40cf-8350-6fccb723cf34",
                   UserName = "admin",
                   FullName = "admin",
                   Email = "admin@cookit.com",
                   NormalizedUserName = "ADMIN",
                   NormalizedEmail = "ADMIN@COOKIT.COM",
                   PasswordHash = "AQAAAAIAAYagAAAAECJp7LhsC6Be7WPS0mPtlJt9Px5Eje2mpCAH+DXXNCr/PfJE2aH3blhK6fpym4YQMA==",  //Admin123!
                   EmailConfirmed = true,
                   LockoutEnabled = true,
                   PhoneNumberConfirmed = true,
                   SecurityStamp = "4G4YRSV6FEXCB6A7DC2YXLSDI5Q3XXFJ",
                   ConcurrencyStamp = "59a80b94-5765-4bf9-ac42-2cca148f6eb2"
               });

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>()
                {
                    RoleId = "8e445865-a24d-4543-a6c6-9443d048cdb9",
                    UserId = "b8f6982f-8c8a-40cf-8350-6fccb723cf34"
                });
        }
    }
}