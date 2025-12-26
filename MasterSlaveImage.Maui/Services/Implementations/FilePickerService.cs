using MasterSlaveImage.Maui.Services.Interfaces;

namespace MasterSlaveImage.Maui.Services.Implementations;

public class FilePickerService : IFilePickerService
{
	public async Task<IEnumerable<FileResult>> PickImagesAsync()
	{
		var result = await FilePicker.Default.PickMultipleAsync(new PickOptions { FileTypes = FilePickerFileType.Images });
		return result ?? Enumerable.Empty<FileResult>();
	}
}
