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
    public partial class ActorViewModel : ObservableObject
    {
        private static readonly Lazy<ActorViewModel> _instance = new(() => new ActorViewModel());

        public static ActorViewModel Instance => _instance.Value;

        private ActorViewModel()
        {
            LoadData();
        }

        [ObservableProperty]
        ObservableCollection<Actor> actors;

        async void LoadData()
        {
            var client = new ApiClient();

            var ActorsResponse = await client.GetActors();
            ActorsResponse.Insert(0, new Actor());

            foreach (var item in ActorsResponse)
            {
                item.PhotoFilePath = ApiClient.baseUrl + "/storage/" + item.PhotoFilePath;
            }

            Actors = new ObservableCollection<Actor>(ActorsResponse);
        }

        [RelayCommand]
        async Task AddPhotoFilePath(Actor item)
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

                    item.PhotoFilePath = filePath;  // Убедитесь, что Actor реализует INotifyPropertyChanged
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выборе файла: {ex.Message}");
            }
        }


        [RelayCommand]
        async void Add(Actor item)
        {
            var client = new ApiClient();
            if (Actors.IndexOf(item) == 0)
            {
                await client.AddActor(item, item.PhotoFilePath);
                LoadData();
                return;
            }
            await client.UpdateActor(item);

            LoadData();
        }

        [RelayCommand]
        async void Delete(Actor item)
        {
            var client = new ApiClient();
            client.DeleteActor(item.Id);
            Actors.Remove(item);
        }
    }
}
