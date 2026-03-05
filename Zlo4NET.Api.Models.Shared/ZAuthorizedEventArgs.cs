using System;
using Zlo4NET.Api.DTO;

namespace Zlo4NET.Api.Models.Shared;

public class ZAuthorizedEventArgs : EventArgs
{
	public ZUserDto AuthorizedUser { get; }

	public ZAuthorizedEventArgs(ZUserDto user)
	{
		AuthorizedUser = user;
	}
}
