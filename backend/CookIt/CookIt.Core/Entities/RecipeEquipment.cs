namespace CookIt.Core.Entities
{
    public class RecipeEquipment
    {
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
    }
}
