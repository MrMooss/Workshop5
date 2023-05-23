using System.ComponentModel.DataAnnotations;

namespace Workshop5.Models
{
    public class Food
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Recipe { get; set; }

        public string ImageUrl { get; set; }

        public Food()
        {
            Id = Guid.NewGuid().ToString();
        }

    }
}

