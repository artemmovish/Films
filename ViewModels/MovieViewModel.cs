using Admin.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.ViewModels
{
    public partial class MovieViewModel : ObservableObject
    {
        private static readonly Lazy<MovieViewModel> _instance = new(() => new MovieViewModel());

        public static MovieViewModel Instance => _instance.Value;

        private MovieViewModel()
        {

        }

        [ObservableProperty]
        ObservableCollection<Movie> movies;

        [ObservableProperty]
        ObservableCollection<Studio> studios;

        [ObservableProperty]
        ObservableCollection<Genre> genres;

        [ObservableProperty]
        ObservableCollection<Genre> selectedGenre;

        [ObservableProperty]
        ObservableCollection<AgeRating> ageRatings;

        public async Task InitializeAsync()
        {
            LoadData();
        }

        async void LoadData()
        {
            var client = new ApiClient();

            if (!ApiClient.Auth)
            {
                await client.Login();
            }

            var genreResponse = await client.GetGenres();
            this.Genres = new ObservableCollection<Genre>(genreResponse);

            var studioResponse = await client.GetStudios();
            Studios = new ObservableCollection<Studio>(studioResponse);

            List<AgeRating> ageRatingsResponse = new List<AgeRating>()
        {
            new AgeRating() { Id = 1, Age = 6 },
            new AgeRating() { Id = 2, Age = 12 },
            new AgeRating() { Id = 3, Age = 16 },
            new AgeRating() { Id = 4, Age = 18 },
        };

            AgeRatings = new ObservableCollection<AgeRating>(ageRatingsResponse);

            var moviesResponse = await client.GetMovies();
            moviesResponse.Insert(0, new Movie());

            Movies = new ObservableCollection<Movie>(moviesResponse);

            foreach (var item in Movies)
            {
                item.Photo = ApiClient.baseUrl + "/storage/" + item.Photo;
            }

            Movies = new ObservableCollection<Movie>(Movies);
        }

        [RelayCommand]
        async Task AddMoviesPhoto(Movie item)
        {
            if (item == null)
            {
                Console.WriteLine("Ошибка: объект фильма не задан.");
                return;
            }

            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите PNG изображение",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    var filePath = Path.Combine(FileSystem.CacheDirectory, result.FileName);
                    using var newFile = File.Create(filePath);
                    await stream.CopyToAsync(newFile);

                    item.Photo = filePath;  // Убедитесь, что Movie реализует INotifyPropertyChanged
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выборе файла: {ex.Message}");
            }
        }


        [RelayCommand]
        async void AddMovies(Movie item)
        {
            var client = new ApiClient();
            if (Movies.IndexOf(item) == 0)
            {
                await client.AddMovie(item, item.Photo);
                LoadData();
                return;
            }
            await client.UpdateMovie(item);

            LoadData();
        }

        [RelayCommand]
        async void DeleteMovie(Movie item)
        {
            var client = new ApiClient();
            client.DeleteMovie(item.Id);
            Movies.Remove(item);
        }
    }

}
