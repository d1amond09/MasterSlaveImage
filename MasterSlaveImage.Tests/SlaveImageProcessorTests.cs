using MasterSlaveImage.SlaveNode.Application.Implementations;
using MasterSlaveImage.SlaveNode.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;

namespace MasterSlaveImage.Tests;

public class SlaveImageProcessorTests
{
	private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
	private readonly ImageProcessor _processor;

	public SlaveImageProcessorTests()
	{
		_loggerMock = new Mock<ILogger<ImageProcessor>>();
		_processor = new ImageProcessor(_loggerMock.Object);
	}

	[Fact]
	public void ProcessImage_ShouldReturnError_WhenBytesAreInvalid()
	{
		var invalidBytes = new byte[] { 0x00, 0x01, 0x02 };
		var result = _processor.ProcessImage(invalidBytes, 100, 100, "jpg");

		result.ProcessedBytes.Should().BeNull();
		result.ErrorMessage.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void ProcessImage_ShouldReturnError_WhenFormatIsUnsupported()
	{
		var dummyBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
		var result = _processor.ProcessImage(dummyBytes, 100, 100, "tiff");

		result.ErrorMessage.Should().Contain("Image processing failed");
	}

}

public class Folder
{
	public string Path { get; }
	public string Name { get; }
	public Folder(string path, string name) { Path = path; Name = name; }
}