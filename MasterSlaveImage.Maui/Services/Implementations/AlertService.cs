using MasterSlaveImage.Maui.Services.Interfaces;

namespace MasterSlaveImage.Maui.Services.Implementations;

public class AlertService : IAlertService
{
	public async Task ShowAlertAsync(string title, string message, string cancel)
	{
		if (Application.Current?.MainPage != null)
		{
			await Application.Current.MainPage.DisplayAlert(title, message, cancel);
		}
	}

	public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Да", string cancel = "Нет")
	{
		if (Application.Current?.MainPage != null)
		{
			return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
		}
		return false;
	}
}
