namespace CookIt.Core.Entities
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public List<RecipeEquipment> RecipeEquipments { get; set; } = new();
    }
}
