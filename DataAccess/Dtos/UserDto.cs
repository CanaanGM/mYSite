// Ignore Spelling: Dtos Dto

namespace DataAccess.Dtos
{
    public class UserReadDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? ProfilePicture { get; set; }
    }
}