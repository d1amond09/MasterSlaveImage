namespace MasterSlaveImage.Shared.Contracts;

public record SlaveRequest(byte[] ImageBytes, int Width, int Height, string Format, string OriginalFileName);
