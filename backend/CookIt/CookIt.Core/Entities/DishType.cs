namespace CookIt.Core.Entities
{
    public class DishType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

    }
}
