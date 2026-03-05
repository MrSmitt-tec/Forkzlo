namespace Zlo4NET.Api.Models.Shared;

public class ZMultiParams : ZBaseParameters
{
	public uint ServerId { get; set; }

	public ZRole Role { get; set; }

	public string Password { get; set; }
}
