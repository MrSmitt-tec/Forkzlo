using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Zlo4NET.Api.Models.Shared;

namespace Zlo4NET.Api.Service;

public interface IZGameProcess
{
	Process GameProcess { get; }

	bool IsRun { get; }

	event EventHandler<ZGamePipeArgs> StateChanged;

	Task<ZRunResult> RunAsync();

	bool TryUnfoldGameWindow();

	bool TryClose();
}
