using MasterSlaveImage.SlaveNode.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace MasterSlaveImage.SlaveNode.Application.Implementations;

public class ImageProcessor(ILogger<ImageProcessor> logger) : IImageProcessor
{
	private readonly ILogger<ImageProcessor> _logger = logger;

	public (byte[]? ProcessedBytes, string? ErrorMessage) ProcessImage(byte[] sourceBytes, int width, int height, string format)
	{
		try
		{
			using var image = Image.Load(sourceBytes);

			image.Mutate(x => x.Resize(new ResizeOptions
			{
				Size = new Size(width, height),
				Mode = ResizeMode.Crop
			}));

			using var outputStream = new MemoryStream();

			switch (format.ToLowerInvariant())
			{
				case "jpg":
				case "jpeg":
					image.SaveAsJpeg(outputStream, new JpegEncoder { Quality = 85 });
					break;
				case "png":
					image.SaveAsPng(outputStream, new PngEncoder());
					break;
				case "bmp":
					image.SaveAsBmp(outputStream, new BmpEncoder());
					break;
				default:
					return (null, $"Unsupported target format: {format}");
			}

			return (outputStream.ToArray(), null);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при обработке изображения.");
			return (null, $"Image processing failed: {ex.Message}");
		}
	}
}
