namespace MasterSlaveImage.Shared.Contracts;

public record SlaveResponse(bool Success, byte[]? ProcessedImageBytes, string ErrorMessage);
