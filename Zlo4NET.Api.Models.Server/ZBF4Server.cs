using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Api.Models.Server;

public class ZBF4Server : ZServerBase
{
	[ZObservableProperty]
	[ZMapperProperty]
	public override byte SpectatorsCapacity { get; set; }
}
