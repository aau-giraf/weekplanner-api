namespace GirafAPI.Entities.Users.DTOs
{
    public record LoginDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
