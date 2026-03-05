namespace Zlo4NET.Core.ZClientAPI;

internal enum ZCommand : byte
{
	Ping = 0,
	UserInfo = 1,
	PlayerInfo = 2,
	ServerList = 3,
	Stats = 4,
	Items = 5,
	Packs = 6,
	Inject = 7,
	GameList = 8,
	RunGame = 9,
	None = byte.MaxValue
}
