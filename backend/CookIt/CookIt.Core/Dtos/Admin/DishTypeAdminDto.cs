using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Admin
{
    public class DishTypeAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public bool IsUsedInRecipes { get; set; }
    }

    public class CreateDishTypeDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateDishTypeDto : CreateDishTypeDto
    {
        [Required]
        public int Id { get; set; }
    }
}