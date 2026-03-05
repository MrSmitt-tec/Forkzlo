using System.Collections.Generic;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Api.Models.Server;

public class ZBFHAttributes : ZAttributesBase
{
	[ZObservableProperty]
	public override string FairFight => _getValue("fairfight");

	public override string TickRate => _getValue("tickrate");

	public ZBFHAttributes(IDictionary<string, string> attributes)
		: base(attributes)
	{
	}
}
