using System;
using Zlo4NET.Api.Models.Shared;

namespace Zlo4NET.Api.Service;

public interface IZLogger
{
	event EventHandler<ZLogMessageArgs> LogMessage;

	void SetLogLevelFiltering(ZLogLevel level);
}
