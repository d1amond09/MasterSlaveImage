using System.IO.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MasterSlaveImage.Tests.Helpers;

public static class PayloadGenerator
{
	public static string Generate(int count)
	{
		string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);
		string json = "{ \"Width\": 1920, \"Height\": 1080, \"Format\": \"png\" }";
		File.WriteAllText(Path.Combine(tempDir, "task.json"), json);

		Parallel.For(0, count, i =>
		{
			int size = 2000;
			using var image = new Image<Rgba32>(size, size);
			for (int y = 0; y < size; y+=10)
				for (int x = 0; x < size; x+=10)
					image[x, y] = Color.Red;
			image.SaveAsJpeg(Path.Combine(tempDir, $"img_{i}.jpg"));
		});

		string zipPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.zip");
		ZipFile.CreateFromDirectory(tempDir, zipPath);
		Directory.Delete(tempDir, true);
		return zipPath;
	}
}