using System;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Api.Service;

namespace Zlo4NET.Core.Data;

internal class ZLogger : IZLogger
{
	private static readonly Lazy<ZLogger> __lazyInstance;

	private ZLogLevel _levelFilter = ZLogLevel.Warning | ZLogLevel.Error;

	private string _lastLogMessage;

	public static ZLogger Instance => __lazyInstance.Value;

	public event EventHandler<ZLogMessageArgs> LogMessage;

	static ZLogger()
	{
		__lazyInstance = new Lazy<ZLogger>(() => new ZLogger(), isThreadSafe: true);
	}

	private void OnLogMessage(ZLogLevel level, string message, bool passDuplicates)
	{
		if ((_lastLogMessage != message || passDuplicates) && _levelFilter.HasFlag(level))
		{
			this.LogMessage?.Invoke(this, new ZLogMessageArgs(level, message));
			_lastLogMessage = message;
		}
	}

	public void Debug(string message, bool passDuplicates = false)
	{
		OnLogMessage(ZLogLevel.Debug, message, passDuplicates);
	}

	public void Info(string message, bool passDuplicates = false)
	{
		OnLogMessage(ZLogLevel.Info, message, passDuplicates);
	}

	public void Warning(string message, bool passDuplicates = false)
	{
		OnLogMessage(ZLogLevel.Warning, message, passDuplicates);
	}

	public void Error(string message, bool passDuplicates = false)
	{
		OnLogMessage(ZLogLevel.Error, message, passDuplicates);
	}

	public void Log(ZLogLevel level, string message, bool passDuplicates = false)
	{
		OnLogMessage(level, message, passDuplicates);
	}

	public void SetLogLevelFiltering(ZLogLevel level)
	{
		_levelFilter = level;
	}
}
