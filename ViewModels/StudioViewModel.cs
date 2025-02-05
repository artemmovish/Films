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
    public partial class StudioViewModel : ObservableObject
    {
        private static readonly Lazy<StudioViewModel> _instance = new(() => new StudioViewModel());

        public static StudioViewModel Instance => _instance.Value;

        [ObservableProperty]
        ObservableCollection<Studio> studios;

        private StudioViewModel()
        {
            LoadData();
        }

        async Task LoadData()
        {
            var client = new ApiClient();

            if (!ApiClient.Auth)
            {
                await client.Login();
            }

            Studios = new ObservableCollection<Studio>(await client.GetStudios());
            Studios.Insert(0, new Studio() );
        }

        [RelayCommand]
        async void Add(Studio studio)
        {
            var client = new ApiClient();

            if (studios.IndexOf(studio) == 0)
            {
                await client.AddStudio(studio);
                LoadData();
                return;
            }
            await client.UpdateStudio(studio);
            LoadData();
        }
        [RelayCommand]
        async void Delete(Studio studio)
        {
            var client = new ApiClient();
            client.DeleteStudio(studio.Id);
            Studios.Remove(studio);
        }

    }
}
