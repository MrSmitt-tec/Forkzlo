using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Extensions;
using Zlo4NET.Core.Helpers;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data.Parsers;

internal class ZInstalledGamesParser : IZInstalledGamesParser
{
	public ZInstalledGames Parse(ZPacket packet)
	{
		ZInstalledGames zInstalledGames = new ZInstalledGames();
		List<ZInstalledGame> list;
		using (MemoryStream input = new MemoryStream(packet.Payload, writable: false))
		{
			using BinaryReader binaryReader = new BinaryReader(input, Encoding.ASCII);
			zInstalledGames.IsX64 = binaryReader.ReadBoolean();
			uint num = binaryReader.ReadZUInt32();
			list = new List<ZInstalledGame>((int)num);
			for (int i = 0; i < num; i++)
			{
				ZInstalledGame zInstalledGame = new ZInstalledGame
				{
					RunnableName = binaryReader.ReadZString(),
					ZloName = binaryReader.ReadZString(),
					FriendlyName = binaryReader.ReadZString()
				};
				zInstalledGame.EnumGame = ZStringToGameConverter.Convert(zInstalledGame.ZloName);
				list.Add(zInstalledGame);
			}
		}
		zInstalledGames.InstalledGames = list.Where((ZInstalledGame g) => g.EnumGame != ZGame.None).ToArray();
		return zInstalledGames;
	}
}
