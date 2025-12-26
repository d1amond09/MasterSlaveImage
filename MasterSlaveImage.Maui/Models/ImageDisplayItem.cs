using CommunityToolkit.Mvvm.ComponentModel;

namespace MasterSlaveImage.Maui.Models;

public partial class ImageDisplayItem : ObservableObject
{
	[ObservableProperty] private ImageSource? originalSource;
	[ObservableProperty] private ImageSource? processedSource;
	[ObservableProperty] private string fileName;
	[ObservableProperty] private string fileTypeBadge;
	public byte[]? ProcessedBytes { get; set; }
}