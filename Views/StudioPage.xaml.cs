using Admin.ViewModels;

namespace Admin.Views;

public partial class StudioPage : ContentPage
{
	public StudioPage()
	{
		InitializeComponent();

		BindingContext = StudioViewModel.Instance;
	}
}