namespace MasterSlaveImage.Maui.Services.Interfaces;

public interface IAlertService
{
	Task ShowAlertAsync(string title, string message, string cancel = "OK");
	Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Да", string cancel = "Нет");
}