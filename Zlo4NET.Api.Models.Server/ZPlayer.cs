using Zlo4NET.Api.Models.Shared;

namespace Zlo4NET.Api.Models.Server;

public class ZPlayer
{
	public byte Slot { get; set; }

	public uint Id { get; set; }

	public string Name { get; set; }

	public ZPlayerRole Role { get; set; }
}
