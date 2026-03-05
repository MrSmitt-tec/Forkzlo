using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Zlo4NET.Api.Models.Server;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Extensions;
using Zlo4NET.Core.Helpers;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data.Parsers;

internal class ZServersListParser : IZServersListParser
{
	private const int __mapNameIndex = 0;

	private const int __gameModeIndex = 1;

	private const string __threadName = "sl_parse";

	private readonly object _sync = new object();

	private readonly object _threadInstanceLock = new object();

	private readonly IEnumerable<ZPacket> _emptyEnumerable = CollectionHelper.GetEmptyEnumerable<ZPacket>();

	private readonly uint _myId;

	private readonly ZGame _gameContext;

	private readonly ZMapNameConverter _mapConverter;

	private readonly ZGameModesConverter _gameModeConverter;

	private readonly Queue<ZPacket[]> _cache;

	private readonly ZLogger _logger;

	private Thread _thread;

	public Action<ZServerBase, ZServerParserAction> ResultCallback { get; set; }

	private ZAttributesBase BuildAttributes(IDictionary<string, string> attributes, ZGame game)
	{
		ZAttributesBase result = null;
		switch (game)
		{
		case ZGame.BF3:
			result = new ZBF3Attributes(attributes);
			break;
		case ZGame.BF4:
			result = new ZBF4Attributes(attributes);
			break;
		case ZGame.BFH:
			result = new ZBFHAttributes(attributes);
			break;
		}
		return result;
	}

	public ZServerBase BuildServerModel(ZGame game, uint serverId)
	{
		ZServerBase zServerBase = null;
		switch (game)
		{
		case ZGame.BF3:
			zServerBase = new ZBF3Server
			{
				Id = serverId
			};
			break;
		case ZGame.BF4:
			zServerBase = new ZBF4Server
			{
				Id = serverId
			};
			break;
		case ZGame.BFH:
			zServerBase = new ZBFHServer
			{
				Id = serverId
			};
			break;
		}
		zServerBase.Game = game;
		return zServerBase;
	}

	public void ParsePlayers(uint id, ZServerBase model, BinaryReader reader)
	{
		ObservableCollection<ZPlayer> observableCollection = new ObservableCollection<ZPlayer>();
		byte b = reader.ReadByte();
		for (byte b2 = 0; b2 < b; b2++)
		{
			ZPlayer item = new ZPlayer
			{
				Slot = reader.ReadByte(),
				Id = reader.ReadZUInt32(),
				Name = reader.ReadZString(),
				Role = ZPlayerRole.Other
			};
			observableCollection.Add(item);
		}
		ZPlayer zPlayer = observableCollection.FirstOrDefault((ZPlayer p) => p.Id == id);
		if (zPlayer != null)
		{
			zPlayer.Role = ZPlayerRole.IAm;
		}
		model.Players = observableCollection;
		model.CurrentPlayersNumber = (byte)observableCollection.Count;
	}

	public void ParseIntoServerModel(ZServerBase model, BinaryReader reader)
	{
		model.Ip = UIntToIPAddress.Convert(reader.ReadZUInt32());
		model.Port = reader.ReadZUInt16();
		reader.SkipBytes(6);
		byte b = reader.ReadByte();
		Dictionary<string, string> dictionary = new Dictionary<string, string>(b);
		for (int i = 0; i < b; i++)
		{
			string key = reader.ReadZString().ToLowerInvariant();
			string value = reader.ReadZString();
			dictionary.Add(key, value);
		}
		model.Name = reader.ReadZString();
		model.Players = new ObservableCollection<ZPlayer>();
		reader.SkipBytes(17);
		reader.SkipZString();
		reader.SkipBytes(6);
		reader.SkipZString();
		reader.SkipBytes(1);
		reader.SkipZString();
		switch (model.Game)
		{
		case ZGame.BF3:
			model.PlayersCapacity = reader.ReadByte();
			reader.SkipBytes(4);
			break;
		case ZGame.BF4:
		case ZGame.BFH:
		{
			reader.SkipBytes(4);
			byte b2 = reader.ReadByte();
			reader.SkipBytes(1);
			model.SpectatorsCapacity = reader.ReadByte();
			model.PlayersCapacity = (byte)(b2 - model.SpectatorsCapacity);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		IDictionary<string, string> attributes = _NormalizeAttributes(dictionary);
		model.Attributes = BuildAttributes(attributes, model.Game);
		model.Settings = BuildSettings(attributes);
		model.MapRotation = BuildMapRotation(model.Game, attributes);
		model.Ping = ZPingService.GetPing(model.Ip);
	}

	private IDictionary<string, string> BuildSettings(IDictionary<string, string> attributes)
	{
		return (from s in attributes["settings"].Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)
			select s.Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary((string[] s) => s.First(), (string[] s) => s.Last());
	}

	private ZMapRotation BuildMapRotation(ZGame game, IDictionary<string, string> attributes)
	{
		ZMapRotation zMapRotation = new ZMapRotation(null);
		List<ZMap> list = _ParseMapList(attributes["maps"], game);
		int[] array = _ParseMapIndexes(attributes.ContainsKey("mapsinfo") ? attributes["mapsinfo"] : string.Empty);
		ZMap currentMap = new ZMap
		{
			Name = (attributes.ContainsKey("level") ? _mapConverter.GetMapNameByKey(game, attributes["level"]) : "NA"),
			GameModeName = (attributes.ContainsKey("levellocation") ? _gameModeConverter.GetGameModeNameByKey(game, attributes["levellocation"]) : "NA"),
			Role = ZMapRole.Current
		};
		zMapRotation.Current = currentMap;
		if (array != null)
		{
			int num = array.Last();
			if (num <= list.Count - 1)
			{
				ZMap zMap = list[num];
				zMap.Role = ZMapRole.Next;
				zMapRotation.Next = zMap;
			}
		}
		ZMap zMap2 = list.FirstOrDefault((ZMap m) => m.Name == currentMap.Name && m.GameModeName == currentMap.GameModeName);
		if (zMap2 == null)
		{
			list.Add(currentMap);
		}
		else
		{
			zMap2.Role = ZMapRole.Current;
		}
		zMapRotation.Rotation = new ObservableCollection<ZMap>(list);
		attributes.Remove("maps");
		attributes.Remove("mapsinfo");
		attributes.Remove("level");
		attributes.Remove("levellocation");
		return zMapRotation;
	}

	private int[] _ParseMapIndexes(string mapsInfoString)
	{
		if (!string.IsNullOrEmpty(mapsInfoString))
		{
			return mapsInfoString.Split(';').Last().Split(',')
				.Select(int.Parse)
				.ToArray();
		}
		return null;
	}

	private List<ZMap> _ParseMapList(string mapString, ZGame game)
	{
		return (from str in mapString.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)
			select str.Split(',') into a
			where a.Length > 1
			select new ZMap
			{
				Name = _mapConverter.GetMapNameByKey(game, a[0]),
				GameModeName = _gameModeConverter.GetGameModeNameByKey(game, a[1]),
				Role = ZMapRole.Other
			}).ToList();
	}

	private IDictionary<string, string> _NormalizeAttributes(IDictionary<string, string> rawAttributes)
	{
		IGrouping<string, string>[] array = (from key in rawAttributes.Keys
			where key.Any(char.IsDigit)
			group key by key.Substring(0, key.Length - 1)).ToArray();
		foreach (IGrouping<string, string> grouping in array)
		{
			string value = grouping.Aggregate(string.Empty, (string current, string key) => current + rawAttributes[key]);
			grouping.ToList().ForEach(delegate(string k)
			{
				rawAttributes.Remove(k);
			});
			rawAttributes.Add(grouping.Key, value);
		}
		return rawAttributes;
	}

	public ZServersListParser(uint myId, ZGame gameContext, ZLogger logger)
	{
		_cache = new Queue<ZPacket[]>(5);
		_mapConverter = new ZMapNameConverter();
		_gameModeConverter = new ZGameModesConverter();
		_myId = myId;
		_gameContext = gameContext;
		_logger = logger;
	}

	private void _parseLoop()
	{
		while (true)
		{
			IEnumerable<ZPacket> enumerable = _emptyEnumerable;
			lock (_sync)
			{
				if (_cache.Any())
				{
					enumerable = _cache.Dequeue();
				}
			}
			foreach (ZPacket item in enumerable)
			{
				using MemoryStream input = new MemoryStream(item.Payload, writable: false);
				using BinaryReader binaryReader = new BinaryReader(input, Encoding.ASCII);
				ZServerParserAction zServerParserAction = (ZServerParserAction)binaryReader.ReadByte();
				ZGame zGame = (ZGame)binaryReader.ReadByte();
				uint serverId = binaryReader.ReadZUInt32();
				ZServerBase zServerBase = BuildServerModel(zGame, serverId);
				if (_gameContext != zGame)
				{
					ResultCallback?.Invoke(null, ZServerParserAction.Ignore);
					continue;
				}
				switch (zServerParserAction)
				{
				case ZServerParserAction.Add:
					ParseIntoServerModel(zServerBase, binaryReader);
					break;
				case ZServerParserAction.PlayersList:
					ParsePlayers(_myId, zServerBase, binaryReader);
					break;
				}
				ResultCallback?.Invoke(zServerBase, zServerParserAction);
			}
			Thread.Sleep(100);
		}
	}

	public void ParseAsync(ZPacket[] packets)
	{
		lock (_threadInstanceLock)
		{
			if (_thread == null || !_thread.IsAlive)
			{
				_thread = new Thread(_parseLoop)
				{
					IsBackground = true,
					Name = "sl_parse"
				};
				_thread.Start();
				_logger.Debug("Created a thread for parsing the server list");
			}
		}
		lock (_sync)
		{
			_cache.Enqueue(packets);
			_logger.Debug($"Incoming packets count: {packets.Length}");
		}
	}

	public void Close()
	{
		_thread?.Abort();
		_logger.Debug("Thread for the server list parsing was aborted");
	}
}
