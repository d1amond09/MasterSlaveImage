using MasterSlaveImage.Maui.ViewModels;

namespace MasterSlaveImage.Maui;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
		BindingContext = viewModel;
	}
}
