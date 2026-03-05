using System;

namespace Zlo4NET.Api.Models.Shared;

public class ZLogMessageArgs : EventArgs
{
	public ZLogLevel Level { get; }

	public string Message { get; }

	public ZLogMessageArgs(ZLogLevel level, string message)
	{
		Level = level;
		Message = message;
	}
}
