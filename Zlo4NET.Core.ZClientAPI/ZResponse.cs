namespace Zlo4NET.Core.ZClientAPI;

internal class ZResponse
{
	public ZResponseStatusCode StatusCode { get; set; }

	public ZPacket[] ResponsePackets { get; set; }

	public ZRequest Request { get; }

	public ZResponse(ZRequest request)
	{
		Request = request;
	}
}
