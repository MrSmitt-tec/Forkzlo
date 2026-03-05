using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Zlo4NET.Api.DTO;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Api.Service;
using Zlo4NET.Core.Data.Parsers;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data;

internal class ZConnection : IZConnection
{
	private const int PING_INTERVAL = 15000;

	private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

	private readonly IZUserInfoParser _userInfoParser;

	private readonly System.Timers.Timer _pingTimer;

	private readonly ZLogger _logger;

	private ZUserDto _currentUserInfo;

	private bool _raiseOnConnectionChangedEvent = true;

	private bool? _internalConnectionState;

	public bool IsConnected => _internalConnectionState ?? false;

	public event EventHandler<ZConnectionChangedEventArgs> ConnectionChanged;

	public ZConnection()
	{
		_userInfoParser = ZParsersFactory.CreateUserInfoParser();
		_logger = ZLogger.Instance;
		_pingTimer = new System.Timers.Timer(15000.0)
		{
			Enabled = false,
			AutoReset = true
		};
		_pingTimer.Elapsed += _OnPingTimerElapsedCallback;
		ZRouter.Initialize();
		ZRouter.ConnectionChanged += _OnClientConnectionStateChangedCallback;
	}

	private async void _OnClientConnectionStateChangedCallback(bool clientConnectionState)
	{
		await _semaphore.WaitAsync();
		if (!_internalConnectionState.HasValue || _internalConnectionState.Value != clientConnectionState)
		{
			bool isAuthorized = false;
			if (clientConnectionState)
			{
				ZRequest userRequest = ZRequestFactory.CreateUserInfoRequest();
				ZResponse zResponse = await ZRouter.GetResponseAsync(userRequest);
				if (zResponse.StatusCode == ZResponseStatusCode.Ok)
				{
					_currentUserInfo = _ParseUserInfo(zResponse.ResponsePackets);
					_pingTimer.Start();
					isAuthorized = true;
				}
				else
				{
					_logger.Warning($"Request failed {userRequest}");
				}
				_internalConnectionState = isAuthorized;
			}
			else
			{
				_internalConnectionState = null;
				_currentUserInfo = null;
				_pingTimer.Stop();
			}
			if (_raiseOnConnectionChangedEvent)
			{
				_RaiseOnConnectionChangedEvent(IsConnected, _currentUserInfo);
				_raiseOnConnectionChangedEvent = true;
			}
		}
		_semaphore.Release();
	}

	private ZUserDto _ParseUserInfo(IEnumerable<ZPacket> responsePackets)
	{
		ZPacket packet = responsePackets.Single();
		return _userInfoParser.Parse(packet);
	}

	private async void _OnPingTimerElapsedCallback(object sender, ElapsedEventArgs e)
	{
		if ((await ZRouter.GetResponseAsync(ZRequestFactory.CreatePingRequest())).StatusCode != ZResponseStatusCode.Ok)
		{
			Disconnect();
		}
	}

	private void _RaiseOnConnectionChangedEvent(bool connectionState, ZUserDto authorizedUserDto)
	{
		this.ConnectionChanged?.Invoke(this, new ZConnectionChangedEventArgs(connectionState, authorizedUserDto));
	}

	public void Connect()
	{
		if (!IsConnected)
		{
			ZRouter.Start();
		}
	}

	public void Disconnect(bool raiseEvent = true)
	{
		if (IsConnected)
		{
			_raiseOnConnectionChangedEvent = raiseEvent;
			ZRouter.Stop();
		}
	}

	public ZUserDto GetCurrentUserInfo()
	{
		return _currentUserInfo;
	}
}
