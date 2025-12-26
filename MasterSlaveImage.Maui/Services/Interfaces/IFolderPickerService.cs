namespace MasterSlaveImage.Maui.Services.Interfaces;

public interface IFolderPickerService
{
	Task<string?> PickFolderAsync();
}  