using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Zlo4NET.Api;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Api.Service;
using Zlo4NET.Core.Helpers;
using Zlo4NET.Core.Services;

namespace Zlo4NET.Core.Data;

public class ZApi : IZApi
{
	private static readonly Lazy<IZApi> Lazy;

	private readonly IZConnection _connection;

	private readonly IZInjectorService _injector;

	private readonly IZStatsService _statsService;

	private readonly IZGameFactory _gameFactory;

	private ZConfiguration _config;

	private IZServersListService _lastCreatedServerListInstance;

	public static IZApi Instance => Lazy.Value;

	public IZGameFactory GameFactory => _gameFactory;

	public IZConnection Connection => _connection;

	public IZLogger Logger => ZLogger.Instance;

	static ZApi()
	{
		Lazy = new Lazy<IZApi>(() => new ZApi(), isThreadSafe: true);
	}

	private ZApi()
	{
		_connection = new ZConnection();
		_statsService = new ZStatsService();
		_gameFactory = new ZGameFactory(_connection);
		_injector = new ZInjectorService();
		ZConnectionHelper.Initialize(_connection);
	}

	private async Task<ZStatsBase> _GetStatsImpl(ZGame game)
	{
		return await _statsService.GetStatsAsync(game);
	}

	private IZServersListService _BuildServerListService(ZGame game)
	{
		return new ZServersListService(Connection.GetCurrentUserInfo().UserId, game);
	}

	public Task<ZStatsBase> GetStatsAsync(ZGame game)
	{
		ZConnectionHelper.MakeSureConnection();
		if (game == ZGame.BFH)
		{
			throw new NotSupportedException("Stats not implemented for Battlefield Hardline.");
		}
		return _GetStatsImpl(game);
	}

	public IZServersListService CreateServersListService(ZGame game)
	{
		ZConnectionHelper.MakeSureConnection();
		if (_config == null)
		{
			throw new InvalidOperationException("You cannot create a service until the api is configured.");
		}
		if (game == ZGame.None)
		{
			throw new InvalidEnumArgumentException("game", (int)game, typeof(ZGame));
		}
		if (_lastCreatedServerListInstance != null && _lastCreatedServerListInstance.CanUse)
		{
			_lastCreatedServerListInstance.StopReceiving();
		}
		return _lastCreatedServerListInstance = _BuildServerListService(game);
	}

	public void InjectDll(ZGame game, IEnumerable<string> paths)
	{
		ZConnectionHelper.MakeSureConnection();
		_injector.Inject(game, paths);
	}

	public void Configure(ZConfiguration config)
	{
		if (_config != null)
		{
			throw new InvalidOperationException("This method can only be called once.");
		}
		if (config == null)
		{
			throw new ArgumentNullException("config");
		}
		if (config.SynchronizationContext == null)
		{
			throw new ArgumentException("SynchronizationContext");
		}
		ZSynchronizationWrapper.Initialize(config);
		_config = config;
	}
}
