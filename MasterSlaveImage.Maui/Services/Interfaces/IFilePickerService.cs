namespace MasterSlaveImage.Maui.Services.Interfaces;

public interface IFilePickerService
{
	Task<IEnumerable<FileResult>> PickImagesAsync();
}