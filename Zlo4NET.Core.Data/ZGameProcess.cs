using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Api.Service;
using Zlo4NET.Core.Data.Parsers;
using Zlo4NET.Core.Helpers;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data;

internal class ZGameProcess : IZGameProcess
{
	private readonly IZGameRunParser _parser;

	private readonly IZProcessTracker _processTracker;

	private readonly ZInstalledGame _targetGame;

	private readonly string _runArgs;

	private readonly string _pipeName;

	private readonly ZLogger _logger;

	private _GamePipe _pipe;

	public Process GameProcess => _processTracker.Process;

	public bool IsRun => _processTracker.IsRun;

	public event EventHandler<ZGamePipeArgs> StateChanged;

	public ZGameProcess(string runArgs, ZInstalledGame targetGame, string pipeName, string processName)
	{
		_parser = ZParsersFactory.CreateGameRunInfoParser();
		_runArgs = runArgs;
		_targetGame = targetGame;
		_logger = ZLogger.Instance;
		_pipeName = pipeName;
		_processTracker = new ZProcessTracker(processName, TimeSpan.FromSeconds(1.0), trackAfterLost: false, (Process[] processes) => processes.First());
	}

	public bool TryClose()
	{
		if (!IsRun)
		{
			return false;
		}
		try
		{
			_processTracker.Process.Kill();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public bool TryUnfoldGameWindow()
	{
		if (!IsRun)
		{
			return false;
		}
		bool num = ZUnsafeMethods.SetForegroundWindow(_processTracker.Process.MainWindowHandle);
		bool flag = ZUnsafeMethods.ShowWindow(_processTracker.Process.MainWindowHandle, 1);
		return num && flag;
	}

	public async Task<ZRunResult> RunAsync()
	{
		ZResponse zResponse = await ZRouter.GetResponseAsync(ZRequestFactory.CreateRunGameRequest(_targetGame.RunnableName, _runArgs));
		if (zResponse.StatusCode != ZResponseStatusCode.Ok)
		{
			return ZRunResult.Error;
		}
		ZRunResult num = _parser.Parse(zResponse.ResponsePackets.Single());
		if (num == ZRunResult.Success)
		{
			_pipe = new _GamePipe(_logger, _pipeName);
			_pipe.PipeEvent += _PipeEventHandler;
			_processTracker.ProcessDetected += _ProcessTrackerOnProcessDetected;
			_processTracker.ProcessLost += _ProcessTrackerOnProcessLost;
			_processTracker.StartTrack();
		}
		return num;
	}

	private void _PipeEventHandler(_GameState state)
	{
		_onMessage(state.Event, state.RawEvent, state.States, state.RawState);
	}

	private void _ProcessTrackerOnProcessLost(object sender, EventArgs e)
	{
		_processTracker.StopTrack();
		_processTracker.ProcessDetected -= _ProcessTrackerOnProcessDetected;
		_processTracker.ProcessLost -= _ProcessTrackerOnProcessLost;
		_OnCustomPipeEvent("StateChanged", "State_GameClose");
		_pipe.PipeEvent -= _PipeEventHandler;
		_pipe = null;
	}

	private void _ProcessTrackerOnProcessDetected(object sender, Process e)
	{
		_OnCustomPipeEvent("StateChanged", "State_GameRun");
		_pipe.Begin();
	}

	private void _OnCustomPipeEvent(string eventName, string stateName)
	{
		_GameState gameState = _GameStateParser.ParseStates(eventName, stateName);
		_onMessage(gameState.Event, gameState.RawEvent, gameState.States, gameState.RawState);
	}

	private void _onMessage(ZGameEvent eventEnum, string rawEvent, ZGameState[] stateEnums, string rawState)
	{
		if (this.StateChanged != null)
		{
			Delegate[] invocationList = this.StateChanged.GetInvocationList();
			ZGamePipeArgs e = new ZGamePipeArgs(eventEnum, rawEvent, stateEnums, rawState);
			Delegate[] array = invocationList;
			for (int i = 0; i < array.Length; i++)
			{
				((EventHandler<ZGamePipeArgs>)array[i]).BeginInvoke(this, e, _EndAsyncEvent, null);
			}
		}
	}

	private void _EndAsyncEvent(IAsyncResult iar)
	{
		EventHandler<ZGamePipeArgs> eventHandler = (EventHandler<ZGamePipeArgs>)((AsyncResult)iar).AsyncDelegate;
		try
		{
			eventHandler.EndInvoke(iar);
		}
		catch (Exception ex)
		{
			_logger.Error("Pipe event handler throws exception. MSG: " + ex.Message);
			throw new Exception("Pipe event handler throws exception.", ex);
		}
	}
}
