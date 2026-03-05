using System;

namespace Zlo4NET.Api.Models.Server;

public class ZBF3Server : ZServerBase
{
	public override byte SpectatorsCapacity
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}
}
