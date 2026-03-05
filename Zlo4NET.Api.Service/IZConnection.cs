using System;
using Zlo4NET.Api.DTO;
using Zlo4NET.Api.Models.Shared;

namespace Zlo4NET.Api.Service;

public interface IZConnection
{
	bool IsConnected { get; }

	event EventHandler<ZConnectionChangedEventArgs> ConnectionChanged;

	void Connect();

	void Disconnect(bool raiseEvent = true);

	ZUserDto GetCurrentUserInfo();
}
