using System;
using System.Collections.Generic;

namespace Zlo4NET.Api.Models.Server;

public class ZBF3Attributes : ZAttributesBase
{
	public override string FairFight
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override string TickRate
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public ZBF3Attributes(IDictionary<string, string> attributes)
		: base(attributes)
	{
	}
}
