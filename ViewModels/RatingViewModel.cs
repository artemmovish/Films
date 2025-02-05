using Admin.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace Admin.ViewModels
{
    partial class RatingViewModel : ObservableObject
    {
        private static readonly Lazy<RatingViewModel> _instance = new(() => new RatingViewModel());

        public static RatingViewModel Instance => _instance.Value;

        private RatingViewModel()
        {
            LoadData();
        }

        [ObservableProperty]
        ObservableCollection<MovieRating> movieRatings;

        async void LoadData()
        {
            var client = new ApiClient();
            if (!ApiClient.Auth)
            {
                await client.Login();
            }

            var respone = await client.GetReviews();

            foreach (var item in respone)
            {
                item.Movie.Photo = ApiClient.baseUrl + "/storage/" + item.Movie.Photo;
            }

            MovieRatings = new ObservableCollection<MovieRating>(respone);
        }

        [RelayCommand]
        async void Delete(MovieRating item)
        {
            var client = new ApiClient();
            client.DeleteReview(item.Id);
            MovieRatings.Remove(item);
        }
    }
}
