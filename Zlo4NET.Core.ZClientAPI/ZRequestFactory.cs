using System.Linq;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Helpers;

namespace Zlo4NET.Core.ZClientAPI;

internal static class ZRequestFactory
{
	public static ZRequest CreateUserInfoRequest()
	{
		return new ZRequest
		{
			RequestCommand = ZCommand.UserInfo,
			Method = ZRequestMethod.Get
		};
	}

	public static ZRequest CreatePingRequest()
	{
		return new ZRequest
		{
			RequestCommand = ZCommand.Ping,
			Method = ZRequestMethod.Get
		};
	}

	public static ZRequest CreateServerListOpenStreamRequest(ZGame game)
	{
		ZRequest zRequest = new ZRequest();
		zRequest.RequestCommand = ZCommand.ServerList;
		zRequest.Method = ZRequestMethod.Put;
		zRequest.RequestPayload = new byte[2]
		{
			0,
			(byte)game
		};
		return zRequest;
	}

	public static ZRequest CreateServerListCloseStreamRequest(ZGame game)
	{
		ZRequest zRequest = new ZRequest();
		zRequest.RequestCommand = ZCommand.ServerList;
		zRequest.Method = ZRequestMethod.Put;
		zRequest.RequestPayload = new byte[2]
		{
			1,
			(byte)game
		};
		return zRequest;
	}

	public static ZRequest CreateInstalledGamesRequest()
	{
		return new ZRequest
		{
			RequestCommand = ZCommand.GameList,
			Method = ZRequestMethod.Get
		};
	}

	public static ZRequest CreateRunGameRequest(string runnableGameName, string commandArgs)
	{
		return new ZRequest
		{
			RequestCommand = ZCommand.RunGame,
			Method = ZRequestMethod.Get,
			RequestPayload = ZBitConverter.Convert(runnableGameName).Concat(ZBitConverter.Convert(commandArgs)).ToArray()
		};
	}

	public static ZRequest CreateDllInjectRequest(ZGame game, string dllPath)
	{
		ZRequest zRequest = new ZRequest();
		zRequest.RequestCommand = ZCommand.Inject;
		zRequest.Method = ZRequestMethod.Get;
		zRequest.RequestPayload = new byte[1] { (byte)game }.Concat(ZBitConverter.Convert(dllPath)).ToArray();
		return zRequest;
	}

	public static ZRequest CreateStatsRequest(ZGame game)
	{
		ZRequest zRequest = new ZRequest();
		zRequest.RequestCommand = ZCommand.Stats;
		zRequest.Method = ZRequestMethod.Get;
		zRequest.RequestPayload = new byte[1] { (byte)game };
		return zRequest;
	}
}
