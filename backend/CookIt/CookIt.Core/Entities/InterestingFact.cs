namespace CookIt.Core.Entities
{
    public class InterestingFact
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }                     
        public string FactText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}