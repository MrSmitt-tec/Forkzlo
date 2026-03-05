using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Services;

namespace Zlo4NET.Core.Data.Parsers;

internal static class ZParsersFactory
{
	public static IZUserInfoParser CreateUserInfoParser()
	{
		return new ZUserInfoParser();
	}

	public static IZServersListParser CreateServersListInfoParser(uint myId, ZGame gameContext, ZLogger logger)
	{
		return new ZServersListParser(myId, gameContext, logger);
	}

	public static IZInstalledGamesParser CreateInstalledGamesInfoParser()
	{
		return new ZInstalledGamesParser();
	}

	public static IZGameRunParser CreateGameRunInfoParser()
	{
		return new ZGameRunParser();
	}

	public static IZStatsParser CreateStatsInfoParser()
	{
		return new ZStatsParser();
	}
}
