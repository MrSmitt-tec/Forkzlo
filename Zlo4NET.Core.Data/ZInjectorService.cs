using System.Collections.Generic;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data;

internal class ZInjectorService : IZInjectorService
{
	public async void Inject(ZGame game, IEnumerable<string> dllPaths)
	{
		foreach (string dllPath in dllPaths)
		{
			await ZRouter.GetResponseAsync(ZRequestFactory.CreateDllInjectRequest(game, dllPath));
		}
	}
}
