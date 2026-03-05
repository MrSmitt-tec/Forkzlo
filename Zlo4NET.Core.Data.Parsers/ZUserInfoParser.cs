using System.IO;
using System.Text;
using Zlo4NET.Api.DTO;
using Zlo4NET.Core.Extensions;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data.Parsers;

internal class ZUserInfoParser : IZUserInfoParser
{
	public ZUserDto Parse(ZPacket packet)
	{
		ZUserDto zUserDto = new ZUserDto();
		using MemoryStream input = new MemoryStream(packet.Payload, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input, Encoding.ASCII);
		zUserDto.UserId = binaryReader.ReadZUInt32();
		zUserDto.UserName = binaryReader.ReadZString();
		return zUserDto;
	}
}
