namespace Zlo4NET.Core.ZClientAPI;

internal struct ZPacket
{
	public ZCommand Id { get; set; }

	public int Length
	{
		get
		{
			byte[] payload = Payload;
			if (payload == null)
			{
				return 0;
			}
			return payload.Length;
		}
	}

	public byte[] Payload { get; set; }
}
