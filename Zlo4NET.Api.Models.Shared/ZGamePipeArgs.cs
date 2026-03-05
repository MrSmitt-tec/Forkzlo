using System;
using Zlo4NET.Core.Data;

namespace Zlo4NET.Api.Models.Shared;

public class ZGamePipeArgs : EventArgs
{
	public string RawEvent { get; set; }

	public string RawState { get; set; }

	public string RawFullMessage { get; set; }

	public ZGameEvent Event { get; set; }

	public ZGameState[] States { get; set; }

	public ZGamePipeArgs(ZGameEvent eventEnum, string rawEvent, ZGameState[] stateEnums, string rawState)
	{
		Event = eventEnum;
		States = stateEnums;
		RawEvent = rawEvent;
		RawState = rawState;
		RawFullMessage = rawEvent + " " + rawState;
	}
}
