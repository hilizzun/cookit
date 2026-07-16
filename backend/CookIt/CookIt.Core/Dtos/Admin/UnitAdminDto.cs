using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Admin
{
    public class UnitAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double? ConversionToGrams { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUsedInRecipes { get; set; }
    }

    public class CreateUnitDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public double? ConversionToGrams { get; set; }
    }

    public class UpdateUnitDto : CreateUnitDto
    {
        [Required]
        public int Id { get; set; }
    }
}