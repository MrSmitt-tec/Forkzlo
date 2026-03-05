using System;
using System.Diagnostics;

namespace Zlo4NET.Api.Service;

public interface IZProcessTracker
{
	Process Process { get; }

	bool IsRun { get; }

	string TargetProcessName { get; }

	event EventHandler ProcessLost;

	event EventHandler<Process> ProcessDetected;

	void StartTrack();

	void StopTrack();
}
