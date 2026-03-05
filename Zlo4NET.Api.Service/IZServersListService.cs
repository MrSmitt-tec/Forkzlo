using System;
using System.Collections.ObjectModel;
using Zlo4NET.Api.Models.Server;

namespace Zlo4NET.Api.Service;

public interface IZServersListService : IDisposable
{
	ObservableCollection<ZServerBase> ServersCollection { get; }

	bool CanUse { get; }

	event EventHandler InitialSizeReached;

	void StartReceiving();

	void StopReceiving();
}
