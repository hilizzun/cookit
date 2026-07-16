namespace CookIt.Core.Dtos.Auth
{
    public class BlockUserRequestDto
    {
        public string UserId { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public DateTime? BlockUntil { get; set; } 
    }
}