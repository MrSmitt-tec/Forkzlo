using System;
using System.Collections.ObjectModel;
using System.Linq;
using Zlo4NET.Api.Models.Server;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Api.Service;
using Zlo4NET.Core.Data.Parsers;
using Zlo4NET.Core.Helpers;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data;

internal class ZServersListService : IZServersListService, IDisposable
{
	private readonly ZLogger _logger;

	private readonly IZServersListParser _parser;

	private readonly IZChangesMapper _changesMapper;

	private readonly ZCollectionWrapper _collectionWrapper;

	private int _initialCount;

	private int _initialCountPlayerList;

	private bool __disposed;

	private ZGame _gameContext;

	private bool _isStreamRejected;

	public ObservableCollection<ZServerBase> ServersCollection => _collectionWrapper.Collection;

	public bool CanUse => !__disposed;

	public event EventHandler InitialSizeReached;

	public ZServersListService(uint myId, ZGame game)
	{
		_logger = ZLogger.Instance;
		_parser = ZParsersFactory.CreateServersListInfoParser(myId, game, _logger);
		_parser.ResultCallback = _ActionHandler;
		_gameContext = game;
		_collectionWrapper = new ZCollectionWrapper(new ObservableCollection<ZServerBase>());
		_changesMapper = new ZChangesMapper();
	}

	public void StartReceiving()
	{
		if (__disposed)
		{
			throw new InvalidOperationException("Object disposed.");
		}
		_ = ZRouter.OpenStreamAsync(ZRequestFactory.CreateServerListOpenStreamRequest(_gameContext), _packetsReceivedHandler, _OnStreamRejectedCallback).Result;
	}

	private async void _OnStreamRejectedCallback()
	{
		_isStreamRejected = true;
		await ZRouter.CloseStreamAsync(ZRequestFactory.CreateServerListCloseStreamRequest(_gameContext));
	}

	public void StopReceiving()
	{
		if (__disposed)
		{
			throw new InvalidOperationException("Object disposed.");
		}
		Dispose();
	}

	public void Dispose()
	{
		if (!__disposed)
		{
			if (!_isStreamRejected)
			{
				_ = ZRouter.CloseStreamAsync(ZRequestFactory.CreateServerListCloseStreamRequest(_gameContext)).Result;
			}
			_parser.Close();
			__disposed = true;
		}
	}

	private void _packetsReceivedHandler(ZPacket[] e)
	{
		_parser.ParseAsync(e);
	}

	private void _OnInitialSizeReached()
	{
		this.InitialSizeReached?.Invoke(this, EventArgs.Empty);
	}

	private void _ActionHandler(ZServerBase model, ZServerParserAction action)
	{
		switch (action)
		{
		case ZServerParserAction.Add:
			_AddActionHandler(model);
			_initialCount++;
			break;
		case ZServerParserAction.PlayersList:
			_PlayerListActionHandler(model);
			_initialCountPlayerList++;
			break;
		case ZServerParserAction.Remove:
			_RemoveActionHandler(model);
			break;
		}
		if (_initialCountPlayerList >= _initialCount)
		{
			_OnInitialSizeReached();
		}
	}

	private void _AddActionHandler(ZServerBase model)
	{
		ZServerBase target = _collectionWrapper.Collection.FirstOrDefault((ZServerBase s) => s.Id == model.Id);
		if (target != null)
		{
			ZSynchronizationWrapper.Send<object>(delegate
			{
				ZServerBase zServerBase = model;
				_changesMapper.MapCollection(zServerBase.MapRotation.Rotation, target.MapRotation.Rotation);
				_changesMapper.MapChanges(zServerBase.MapRotation, target.MapRotation);
				_changesMapper.MapChanges(zServerBase, target);
				target.UpdateAll();
			});
		}
		else
		{
			_collectionWrapper.Add(model);
		}
	}

	private void _PlayerListActionHandler(ZServerBase model)
	{
		ZServerBase target = _collectionWrapper.Collection.FirstOrDefault((ZServerBase s) => s.Id == model.Id);
		if (target != null)
		{
			ZSynchronizationWrapper.Send<object>(delegate
			{
				_changesMapper.MapCollection(model.Players, target.Players);
			});
			target.CurrentPlayersNumber = model.CurrentPlayersNumber;
			target.UpdateByName("CurrentPlayersNumber");
		}
		else
		{
			_logger.Warning($"Parsed players for server id: {model.Id} not found this server.", passDuplicates: true);
		}
	}

	private void _RemoveActionHandler(ZServerBase model)
	{
		ZServerBase zServerBase = _collectionWrapper.Collection.FirstOrDefault((ZServerBase s) => s.Id == model.Id);
		if (zServerBase != null)
		{
			_collectionWrapper.Remove(zServerBase);
		}
		else
		{
			_logger.Warning($"Server remove request for id: {model.Id} not found this server.", passDuplicates: true);
		}
	}
}
