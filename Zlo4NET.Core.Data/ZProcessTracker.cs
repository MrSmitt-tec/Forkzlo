using System;
using System.Diagnostics;
using System.Timers;
using Zlo4NET.Api.Service;

namespace Zlo4NET.Core.Data;

public class ZProcessTracker : IZProcessTracker
{
	private readonly Timer _checkLoopRepeater;

	private readonly Func<Process[], Process> _processDetectorFunc;

	private readonly bool _trackAfterLost;

	public Process Process { get; private set; }

	public bool IsRun => Process != null;

	public string TargetProcessName { get; }

	public event EventHandler ProcessLost;

	public event EventHandler<Process> ProcessDetected;

	private void _OnProcessDetected(Process detectedProcess)
	{
		this.ProcessDetected?.Invoke(this, detectedProcess);
	}

	private void _OnProcessLost()
	{
		this.ProcessLost?.Invoke(this, EventArgs.Empty);
	}

	private void _processLostHandler(object sender, EventArgs e)
	{
		StopTrack();
		_OnProcessLost();
		if (_trackAfterLost)
		{
			StartTrack();
		}
	}

	private void _checkHandler(object sender, ElapsedEventArgs e)
	{
		if (IsRun)
		{
			_checkLoopRepeater.Stop();
			return;
		}
		Process[] processesByName = Process.GetProcessesByName(TargetProcessName);
		if (processesByName.Length != 0)
		{
			Process process = _processDetectorFunc(processesByName);
			if (process != null && !process.HasExited)
			{
				Process = process;
				_checkLoopRepeater.Stop();
				process.EnableRaisingEvents = true;
				process.Exited += _processLostHandler;
				_OnProcessDetected(process);
			}
		}
	}

	public ZProcessTracker(string targetProcessName, TimeSpan checkInterval, bool trackAfterLost, Func<Process[], Process> processDetectorFunc)
	{
		_trackAfterLost = trackAfterLost;
		_checkLoopRepeater = new Timer
		{
			Enabled = false,
			Interval = checkInterval.TotalMilliseconds
		};
		_checkLoopRepeater.Elapsed += _checkHandler;
		_processDetectorFunc = processDetectorFunc;
		TargetProcessName = targetProcessName;
	}

	public void StartTrack()
	{
		_checkHandler(this, null);
		_checkLoopRepeater.Start();
	}

	public void StopTrack()
	{
		_checkLoopRepeater.Stop();
		if (Process != null)
		{
			Process.Exited -= _processLostHandler;
			Process.EnableRaisingEvents = false;
		}
		Process = null;
	}
}
