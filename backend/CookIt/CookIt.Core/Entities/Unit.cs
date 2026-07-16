namespace CookIt.Core.Entities
{
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double? ConversionToGrams { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}