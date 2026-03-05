using System.Net;

namespace Zlo4NET.Core.Helpers;

internal static class UIntToIPAddress
{
	internal static IPAddress Convert(uint ipAddress)
	{
		return new IPAddress(ZBitConverter.Convert(ipAddress));
	}
}
