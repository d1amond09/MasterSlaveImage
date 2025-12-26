using System.Security.Claims;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using MasterSlaveImage.Master.Domain;
using MasterSlaveImage.Master.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MasterSlaveImage.Master.Application.Implementations;

public class SimpleMembershipProvider(IOptions<MasterSettings> settings) : IMembershipProvider
{
	private readonly string _user = settings.Value.Ftp.User;
	private readonly string _pass = settings.Value.Ftp.Password;

	public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
	{
		if (username == _user && password == _pass)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimsIdentity.DefaultNameClaimType, username),
				new Claim(ClaimsIdentity.DefaultRoleClaimType, "user")
			};
			var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "custom"));

			return Task.FromResult(new MemberValidationResult(MemberValidationStatus.AuthenticatedUser, user));
		}

		return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
	}
}