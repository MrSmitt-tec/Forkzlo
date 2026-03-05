using System.Collections.Generic;
using System.Threading.Tasks;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Api.Service;

namespace Zlo4NET.Api;

public interface IZApi
{
	IZGameFactory GameFactory { get; }

	IZConnection Connection { get; }

	IZLogger Logger { get; }

	Task<ZStatsBase> GetStatsAsync(ZGame game);

	IZServersListService CreateServersListService(ZGame game);

	void InjectDll(ZGame game, IEnumerable<string> paths);

	void Configure(ZConfiguration config);
}
