using FluentAssertions;
using FubarDev.FtpServer.AccountManagement;
using MasterSlaveImage.Master.Application.Implementations;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace MasterSlaveImage.Tests;

public class SimpleMembershipProviderTests
{
	private readonly Mock<IOptions<MasterSettings>> _optionsMock;
	private readonly SimpleMembershipProvider _provider;

	public SimpleMembershipProviderTests()
	{
		_optionsMock = new Mock<IOptions<MasterSettings>>();
		_optionsMock.Setup(o => o.Value).Returns(new MasterSettings
		{
			Ftp = new FtpSettings
			{
				User = "admin",
				Password = "123"
			}
		});

		_provider = new SimpleMembershipProvider(_optionsMock.Object);
	}

	[Fact]
	public async Task ValidateUserAsync_ShouldReturnSuccess_ForCorrectCredentials()
	{
		var result = await _provider.ValidateUserAsync("admin", "123");
		result.Status.Should().Be(MemberValidationStatus.AuthenticatedUser);
		result.User.Should().NotBeNull();
	}

	[Fact]
	public async Task ValidateUserAsync_ShouldReturnInvalid_ForWrongPassword()
	{
		var result = await _provider.ValidateUserAsync("admin", "wrong");
		result.Status.Should().Be(MemberValidationStatus.InvalidLogin);
	}

	[Fact]
	public async Task ValidateUserAsync_ShouldReturnInvalid_ForWrongUser()
	{
		var result = await _provider.ValidateUserAsync("guest", "123");
		result.Status.Should().Be(MemberValidationStatus.InvalidLogin);
	}

}