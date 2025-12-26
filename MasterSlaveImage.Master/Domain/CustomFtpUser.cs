using System;
using FubarDev.FtpServer.AccountManagement;

namespace MasterSlaveImage.Master.Domain;

public class CustomFtpUser(string name) : IFtpUser
{
	public string Name { get; } = name;

	public bool IsInGroup(string groupName)
	{
		return false;
	}
}
