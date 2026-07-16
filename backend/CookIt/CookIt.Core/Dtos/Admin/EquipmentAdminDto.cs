using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Admin
{
    public class EquipmentAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public bool IsUsedInRecipes { get; set; }
    }

    public class CreateEquipmentDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateEquipmentDto : CreateEquipmentDto
    {
        [Required]
        public int Id { get; set; }
    }
}