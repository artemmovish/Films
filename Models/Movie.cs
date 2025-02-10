
using Admin.ViewModels;

namespace Admin.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? release_year { get; set; }
        public int? Duration { get; set; }
        public string? Description { get; set; }
        public string? Photo { get; set; }
        public int? studio_id { get; set; }
        public int? age_rating_id { get; set; }
        public Studio? Studio { get; set; }
        public AgeRating? age_rating { get; set; }
        public List<MovieRating> Rating { get; set; } = new List<MovieRating>();
        public List<Genre>? genres { get; set; }
        public List<Actor>? actors { get; set; }
        public string? watch_url { get; set; }

        public int StudioIndex
        {
            get => Studio != null ? MovieViewModel.Instance.Studios.IndexOf(Studio) : -1;
            set => Studio = value >= 0 ? MovieViewModel.Instance.Studios[value] : null;
        }

        public int AgeRatingIndex
        {
            get => age_rating != null ? MovieViewModel.Instance.AgeRatings.IndexOf(age_rating) : -1;
            set => age_rating = value >= 0 ? MovieViewModel.Instance.AgeRatings[value] : null;
        }

        MovieViewModel MovieViewModel => MovieViewModel.Instance;
    }
}