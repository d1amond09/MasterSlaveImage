using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MasterSlaveImage.Shared.Contracts;

namespace MasterSlaveImage.Tests;

public class SharedContractsTests
{
	[Fact]
	public void SlaveRequest_Serialization_ShouldWork()
	{
		var request = new SlaveRequest(new byte[] { 1, 2, 3 }, 800, 600, "jpg", "test.jpg");
		var json = JsonSerializer.Serialize(request);
		var deserialized = JsonSerializer.Deserialize<SlaveRequest>(json);

		deserialized.Should().BeEquivalentTo(request);
	}

	[Fact]
	public void SlaveResponse_Serialization_ShouldWork()
	{
		var response = new SlaveResponse(true, new byte[] { 4, 5, 6 }, null);
		var json = JsonSerializer.Serialize(response);
		var deserialized = JsonSerializer.Deserialize<SlaveResponse>(json);

		deserialized.Should().BeEquivalentTo(response);
	}

}
