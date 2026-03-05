using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Data;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Api.Models.Server;

public class ZMap : ZObservableObject
{
	[ZObservableProperty]
	public string Name { get; set; }

	[ZObservableProperty]
	public string GameModeName { get; set; }

	public ZMapRole Role { get; set; }
}
