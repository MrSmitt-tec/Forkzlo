using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Data;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Api.Models.Server;

public abstract class ZServerBase : ZObservableObject
{
	private int _ping;

	public uint Id { get; set; }

	public IPAddress Ip { get; set; }

	public uint Port { get; set; }

	public ZGame Game { get; set; }

	public IDictionary<string, string> Settings { get; set; }

	[ZObservableProperty]
	[ZMapperProperty]
	public string Name { get; set; }

	[ZObservableProperty]
	[ZMapperProperty]
	public byte PlayersCapacity { get; set; }

	[ZObservableProperty]
	public byte CurrentPlayersNumber { get; set; }

	[ZObservableProperty]
	[ZMapperProperty]
	public ZAttributesBase Attributes { get; set; }

	public ObservableCollection<ZPlayer> Players { get; set; }

	public abstract byte SpectatorsCapacity { get; set; }

	public ZMapRotation MapRotation { get; set; }

	public int Ping
	{
		get
		{
			return _ping;
		}
		set
		{
			SetProperty(ref _ping, value, "Ping");
		}
	}
}
