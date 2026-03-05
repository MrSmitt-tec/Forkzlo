using System;
using Zlo4NET.Api.DTO;

namespace Zlo4NET.Api.Models.Shared;

public class ZConnectionChangedEventArgs : EventArgs
{
	public bool IsConnected { get; }

	public ZUserDto AuthorizedUser { get; }

	public ZConnectionChangedEventArgs(bool isConnected, ZUserDto authorizedUserDto)
	{
		IsConnected = isConnected;
		AuthorizedUser = authorizedUserDto;
	}
}
