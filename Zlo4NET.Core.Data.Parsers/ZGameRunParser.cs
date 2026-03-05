using System.IO;
using System.Text;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data.Parsers;

internal class ZGameRunParser : IZGameRunParser
{
	public ZRunResult Parse(ZPacket packet)
	{
		ZRunResult zRunResult = ZRunResult.None;
		using MemoryStream input = new MemoryStream(packet.Payload, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input, Encoding.ASCII);
		return (ZRunResult)binaryReader.ReadByte();
	}
}
