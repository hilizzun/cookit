using AutoMapper;
using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Entities;
using System.Linq;

namespace CookIt.Infrastructure.Configuration.Profiles
{
    internal class RecipeProfile : Profile
    {
        public RecipeProfile()
        {
            CreateMap<Recipe, RecipeDto>()
                .ForMember(dest => dest.RecipeIngredients, opt => opt.MapFrom(src => src.RecipeIngredients))
                .ForMember(dest => dest.Servings, opt => opt.MapFrom(src => src.Servings))
                .ForMember(dest => dest.TotalCalories, opt => opt.MapFrom(src => src.TotalCalories))
                .ForMember(dest => dest.TotalProteins, opt => opt.MapFrom(src => src.TotalProteins))
                .ForMember(dest => dest.TotalFats, opt => opt.MapFrom(src => src.TotalFats))
                .ForMember(dest => dest.TotalCarbohydrates, opt => opt.MapFrom(src => src.TotalCarbohydrates))
                .ForMember(dest => dest.CaloriesPerServing, opt => opt.MapFrom(src => src.CaloriesPerServing))
                .ForMember(dest => dest.ProteinsPerServing, opt => opt.MapFrom(src => src.ProteinsPerServing))
                .ForMember(dest => dest.FatsPerServing, opt => opt.MapFrom(src => src.FatsPerServing))
                .ForMember(dest => dest.CarbohydratesPerServing, opt => opt.MapFrom(src => src.CarbohydratesPerServing))
                .ForMember(dest => dest.CaloriesPer100g, opt => opt.MapFrom(src => src.CaloriesPer100g))
                .ForMember(dest => dest.ProteinsPer100g, opt => opt.MapFrom(src => src.ProteinsPer100g))
                .ForMember(dest => dest.FatsPer100g, opt => opt.MapFrom(src => src.FatsPer100g))
                .ForMember(dest => dest.CarbohydratesPer100g, opt => opt.MapFrom(src => src.CarbohydratesPer100g))
                .ForMember(dest => dest.ApprovedByUsername, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.UserName : null));

            CreateMap<DishType, DishTypeDto>();

            CreateMap<RecipeIngredient, RecipeIngredientDto>()
                .ForMember(dest => dest.IngredientName, opt => opt.MapFrom(src => src.Ingredient.Name))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));

            CreateMap<RecipeEquipment, RecipeEquipmentDto>()
                .ForMember(dest => dest.EquipmentName, opt => opt.MapFrom(src => src.Equipment.Name));

            CreateMap<CreateRecipeDto, Recipe>()
                .ForMember(dest => dest.RecipeIngredients, opt => opt.MapFrom(src => src.RecipeIngredients))
                .ForMember(dest => dest.RecipeEquipments, opt => opt.MapFrom(src =>
                    src.RecipeEquipments.Select(id => new RecipeEquipment { EquipmentId = id }).ToList()))
                .ForMember(dest => dest.Servings, opt => opt.MapFrom(src => src.Servings));

            CreateMap<UpdateRecipeDto, Recipe>()
                .ForMember(dest => dest.RecipeIngredients, opt => opt.Ignore())
                .ForMember(dest => dest.RecipeEquipments, opt => opt.Ignore())
                .ForMember(dest => dest.DishType, opt => opt.Ignore())
                .ForMember(dest => dest.Servings, opt => opt.MapFrom(src => src.Servings));

            CreateMap<RecipeIngredientCreateDto, RecipeIngredient>()
                .ForMember(dest => dest.RecipeId, opt => opt.Ignore())
                .ForMember(dest => dest.Recipe, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredient, opt => opt.Ignore())
                .ForMember(dest => dest.Unit, opt => opt.Ignore());
        }
    }
}