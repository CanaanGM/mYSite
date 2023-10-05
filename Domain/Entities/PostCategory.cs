namespace Domain.Entities
{
    public class PostCategory
    {
        public Guid PostId { get; set; }
        public Guid CategoryId { get; set; }

        public Post Post { get; set; } = null!;
        public Category Category { get; set; } = null!;
    }
}