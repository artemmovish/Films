
using System.Text.Json.Serialization;


namespace Admin.Models
{
public class Studio
{
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Список фильмов, принадлежащих студии
    public List<Movie> Movies { get; set; } = new List<Movie>();

        public override bool Equals(object obj)
        {
            if (obj is Studio other)
            {
                return this.Id == other.Id;
            }
            return false;
        }
    }
}