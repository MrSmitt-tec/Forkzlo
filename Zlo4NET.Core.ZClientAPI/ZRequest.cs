using System;
using System.Linq;
using Zlo4NET.Core.Helpers;

namespace Zlo4NET.Core.ZClientAPI;

internal class ZRequest
{
	public Guid RequestGuid { get; }

	public ZRequestMethod Method { get; set; }

	public ZCommand RequestCommand { get; set; }

	public byte[] RequestPayload { private get; set; } = new byte[0];

	public ZRequest()
	{
		RequestGuid = Guid.NewGuid();
	}

	public byte[] ToByteArray()
	{
		return new byte[1] { (byte)RequestCommand }.Concat(ZBitConverter.Convert(RequestPayload.Length)).Concat(RequestPayload).ToArray();
	}

	public override string ToString()
	{
		return $"Request {RequestGuid} - Method {Method} Command {RequestCommand} Payload {BitConverter.ToString(RequestPayload)}";
	}
}
