using System.Linq;
using System.Threading.Tasks;
using Zlo4NET.Core.Data.Parsers;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data;

internal class ZInstalledGamesService : IZInstalledGamesService
{
	private readonly IZInstalledGamesParser _installedGamesParser;

	private readonly ZLogger _logger;

	public ZInstalledGamesService()
	{
		_installedGamesParser = ZParsersFactory.CreateInstalledGamesInfoParser();
		_logger = ZLogger.Instance;
	}

	public async Task<ZInstalledGames> GetInstalledGamesAsync()
	{
		ZInstalledGames installedGames = null;
		ZRequest request = ZRequestFactory.CreateInstalledGamesRequest();
		ZResponse zResponse = await ZRouter.GetResponseAsync(request);
		if (zResponse.StatusCode != ZResponseStatusCode.Ok)
		{
			_logger.Warning($"Request fail {request}");
		}
		else
		{
			ZPacket packet = zResponse.ResponsePackets.Single();
			installedGames = _installedGamesParser.Parse(packet);
		}
		return installedGames;
	}
}
