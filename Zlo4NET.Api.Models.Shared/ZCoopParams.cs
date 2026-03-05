using System;

namespace Zlo4NET.Api.Models.Shared;

public class ZCoopParams : ZBaseParameters
{
	public ZPlayMode Mode { get; set; }

	public ZCoopLevels? Level { get; set; }

	public ZCoopDifficulty? Difficulty { get; set; }

	public uint? FriendId { get; set; }

	public override ZGame Game
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
