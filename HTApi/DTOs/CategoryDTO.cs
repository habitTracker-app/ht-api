using HTAPI.Models.Challenges;

namespace HTApi.DTOs
{
    public class CategoryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public CategoryDTO(ChallengeCategory c)
        {
            this.Id = c.Id;
            this.Name = c.Name;
            this.Description = c.Description;
        }
    }
}
