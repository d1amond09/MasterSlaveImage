namespace MasterSlaveImage.SlaveNode.Application.Interfaces;

public interface IImageProcessor
{
	(byte[]? ProcessedBytes, string? ErrorMessage) ProcessImage(byte[] sourceBytes, int width, int height, string format);
}
