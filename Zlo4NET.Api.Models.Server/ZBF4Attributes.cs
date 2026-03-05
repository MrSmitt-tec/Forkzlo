using System.Collections.Generic;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Api.Models.Server;

public class ZBF4Attributes : ZAttributesBase
{
	[ZObservableProperty]
	public override string FairFight => _getValue("fairfight");

	public override string TickRate => _getValue("tickrate");

	public ZBF4Attributes(IDictionary<string, string> attributes)
		: base(attributes)
	{
	}
}
