using Admin.ViewModels;

namespace Admin.Views;

public partial class ActorPage : ContentPage
{
	public ActorPage()
	{
		InitializeComponent();

		BindingContext = ActorViewModel.Instance;
	}
}