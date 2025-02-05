using Admin.ViewModels;

namespace Admin.Views;


public partial class FilmsPage : ContentPage
{
    public FilmsPage()
    {
        BindingContext = MovieViewModel.Instance;
        Task.Run(async () => await MovieViewModel.Instance.InitializeAsync());
        Thread.Sleep(5000);
        InitializeComponent();
        
    }
}