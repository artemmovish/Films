using Admin.ViewModels;

namespace Admin.Views;

public partial class RatingPage : ContentPage
{
	public RatingPage()
	{
		InitializeComponent();

		BindingContext = RatingViewModel.Instance;

    }
}