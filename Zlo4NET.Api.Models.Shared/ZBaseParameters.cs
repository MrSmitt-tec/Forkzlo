namespace Zlo4NET.Api.Models.Shared;

public abstract class ZBaseParameters
{
	public virtual ZGame Game { get; set; } = ZGame.None;

	public virtual ZGameArchitecture? PreferredArchitecture { get; set; }
}
