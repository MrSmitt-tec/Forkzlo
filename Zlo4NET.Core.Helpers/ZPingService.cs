using System.Net;
using System.Net.NetworkInformation;

namespace Zlo4NET.Core.Helpers;

internal static class ZPingService
{
	public static int GetPing(IPAddress address)
	{
		int num = 2;
		try
		{
			while (num > 0)
			{
				PingReply pingReply = new Ping().Send(address, 300);
				if (pingReply != null && pingReply.Status == IPStatus.Success)
				{
					return (int)((pingReply.RoundtripTime > 999) ? 999 : pingReply.RoundtripTime);
				}
				num--;
			}
		}
		catch
		{
		}
		return 999;
	}
}
