using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Zlo4NET.Api.DTO;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Api.Service;
using Zlo4NET.Core.Helpers;
using Zlo4NET.Core.Services;

namespace Zlo4NET.Core.Data;

internal class ZGameFactory : IZGameFactory
{
	private const string _SingleKey = "single";

	private const string _MultiKey = "multi";

	private const string _CoopHostKey = "coop host";

	private const string _CoopJoinKey = "coop join";

	private const string _TestRangeKey = "test range";

	private const string _personaRefReplaceable = "[personaRef]";

	private const string _gameIdReplaceable = "[gameId]";

	private const string _isSpectatorReplaceable = "[isSpectator]";

	private const string _isPasswordReplaceable = "[password]";

	private const string _roleReplaceable = "[role]";

	private const string _friendIdReplaceable = "[friendId]";

	private const string _levelReplaceable = "[level]";

	private const string _difficultyReplaceable = "[difficulty]";

	private const string _spectatorValue = "isspectator=\\\"true\\\"";

	private const string _passwordValue = "password=\\\"{0}\\\"";

	private readonly IZInstalledGamesService _installedGamesService;

	private readonly IZConnection _connection;

	private JObject __runStrings;

	private ZUserDto __userContext => _connection.GetCurrentUserInfo();

	public ZGameFactory(IZConnection connection)
	{
		_installedGamesService = new ZInstalledGamesService();
		_connection = connection;
		_loadRunJSON();
	}

	private IZGameProcess _createRunGame(ZInstalledGame target, string command, ZGame game, ZGameArchitecture architecture)
	{
		return game switch
		{
			ZGame.BF3 => new ZGameProcess(command, target, "venice_snowroller", "bf3"), 
			ZGame.BF4 => new ZGameProcess(command, target, "warsaw_snowroller", (architecture == ZGameArchitecture.x64) ? "bf4" : "bf4_x86"), 
			ZGame.BFH => new ZGameProcess(command, target, "omaha_snowroller", (architecture == ZGameArchitecture.x64) ? "bfh" : "bfh_x86"), 
			_ => throw new Exception(), 
		};
	}

	private void _loadRunJSON()
	{
		using StreamReader streamReader = new StreamReader(ZInternalResource.GetResourceStream("run.json"));
		string json = streamReader.ReadToEnd();
		__runStrings = JObject.Parse(json);
	}

	public async Task<IZGameProcess> CreateSingleAsync(ZSingleParams args)
	{
		ZConnectionHelper.MakeSureConnection();
		ZInstalledGames zInstalledGames = await _installedGamesService.GetInstalledGamesAsync();
		if (zInstalledGames == null)
		{
			throw new Exception("Installed games not received. Check your ZClient connection.");
		}
		ZGameArchitecture architecture = (ZGameArchitecture)(((int?)args.PreferredArchitecture) ?? ((!zInstalledGames.IsX64) ? 1 : 0));
		ZInstalledGame[] array = zInstalledGames.InstalledGames.Where((ZInstalledGame insGame) => insGame.EnumGame == args.Game).ToArray();
		ZInstalledGame zInstalledGame = ((array.Length > 1) ? array.FirstOrDefault((ZInstalledGame insGame) => insGame.RunnableName.EndsWith(architecture.ToString())) : array.FirstOrDefault());
		if (zInstalledGame == null)
		{
			throw new InvalidOperationException($"The target game {args.Game} not found.");
		}
		string text = __runStrings["single"].ToObject<string>();
		text = text.Replace("[personaRef]", __userContext.UserId.ToString());
		return _createRunGame(zInstalledGame, text, args.Game, architecture);
	}

	public async Task<IZGameProcess> CreateCoOpAsync(ZCoopParams args)
	{
		ZConnectionHelper.MakeSureConnection();
		if (args.Mode != ZPlayMode.CooperativeHost && args.Mode != ZPlayMode.CooperativeClient)
		{
			throw new ArgumentException("Mode contains wrong value. Allowed values is CooperativeHost or CooperativeClient.");
		}
		if (ZPlayMode.CooperativeClient == args.Mode && !args.FriendId.HasValue)
		{
			throw new ArgumentException(string.Format("For this {0} mode need to specify {1} value.", args.Mode, "FriendId"));
		}
		if (ZPlayMode.CooperativeHost == args.Mode && (!args.Difficulty.HasValue || !args.Level.HasValue))
		{
			throw new ArgumentException(string.Format("For this {0} mode need to specify {1}, {2} value.", args.Mode, "Difficulty", "Level"));
		}
		ZInstalledGames zInstalledGames = await _installedGamesService.GetInstalledGamesAsync();
		if (zInstalledGames == null)
		{
			throw new Exception("Installed games not received. Check your ZClient connection.");
		}
		ZGameArchitecture architecture = (ZGameArchitecture)(((int?)args.PreferredArchitecture) ?? ((!zInstalledGames.IsX64) ? 1 : 0));
		ZInstalledGame[] array = zInstalledGames.InstalledGames.Where((ZInstalledGame insGame) => insGame.EnumGame == ZGame.BF3).ToArray();
		ZInstalledGame zInstalledGame = ((array.Length > 1) ? array.FirstOrDefault((ZInstalledGame insGame) => insGame.RunnableName.EndsWith(architecture.ToString())) : array.FirstOrDefault());
		if (zInstalledGame == null)
		{
			throw new InvalidOperationException($"The target game {ZGame.BF3} not found.");
		}
		string text;
		if (args.Mode == ZPlayMode.CooperativeHost)
		{
			text = __runStrings["coop host"].ToObject<string>();
			text = text.Replace("[level]", args.Level.ToString().ToUpper());
			text = text.Replace("[difficulty]", args.Difficulty.ToString().ToUpper());
			text = text.Replace("[personaRef]", __userContext.UserId.ToString());
		}
		else
		{
			text = __runStrings["coop join"].ToObject<string>();
			text = text.Replace("[friendId]", args.FriendId.ToString());
			text = text.Replace("[personaRef]", __userContext.UserId.ToString());
		}
		return _createRunGame(zInstalledGame, text, ZGame.BF3, architecture);
	}

	public async Task<IZGameProcess> CreateTestRangeAsync(ZTestRangeParams args)
	{
		ZConnectionHelper.MakeSureConnection();
		if (args.Game == ZGame.BF3)
		{
			throw new NotSupportedException("Battlefield 3 TestRange play mode not supported.");
		}
		if (args.Game == ZGame.BFH)
		{
			throw new NotImplementedException("Battlefield Hardline TestRange not implemented in ZLOEmu.");
		}
		ZInstalledGames zInstalledGames = await _installedGamesService.GetInstalledGamesAsync();
		if (zInstalledGames == null)
		{
			throw new Exception("Installed games not received. Check your ZClient connection.");
		}
		ZGameArchitecture architecture = (ZGameArchitecture)(((int?)args.PreferredArchitecture) ?? ((!zInstalledGames.IsX64) ? 1 : 0));
		ZInstalledGame[] array = zInstalledGames.InstalledGames.Where((ZInstalledGame insGame) => insGame.EnumGame == args.Game).ToArray();
		ZInstalledGame zInstalledGame = ((array.Length > 1) ? array.FirstOrDefault((ZInstalledGame insGame) => insGame.RunnableName.EndsWith(architecture.ToString())) : array.FirstOrDefault());
		if (zInstalledGame == null)
		{
			throw new Exception($"The target game {args.Game} not found.");
		}
		string text = __runStrings["test range"].ToObject<string>();
		text = text.Replace("[personaRef]", __userContext.UserId.ToString());
		return _createRunGame(zInstalledGame, text, args.Game, architecture);
	}

	public async Task<IZGameProcess> CreateMultiAsync(ZMultiParams args)
	{
		ZConnectionHelper.MakeSureConnection();
		if (args.Game == ZGame.BF3 && args.Role == ZRole.Spectator)
		{
			throw new ArgumentException("BF3 is not support Spectator mode.");
		}
		ZInstalledGames zInstalledGames = await _installedGamesService.GetInstalledGamesAsync();
		if (zInstalledGames == null)
		{
			throw new Exception("Installed games not received. Check your ZClient connection.");
		}
		ZGameArchitecture architecture = (ZGameArchitecture)(((int?)args.PreferredArchitecture) ?? ((!zInstalledGames.IsX64) ? 1 : 0));
		ZInstalledGame[] array = zInstalledGames.InstalledGames.Where((ZInstalledGame insGame) => insGame.EnumGame == args.Game).ToArray();
		ZInstalledGame zInstalledGame = ((array.Length > 1) ? array.FirstOrDefault((ZInstalledGame insGame) => insGame.RunnableName.EndsWith(architecture.ToString())) : array.FirstOrDefault());
		if (zInstalledGame == null)
		{
			throw new Exception($"The target game {args.Game} not found.");
		}
		string text = __runStrings["multi"].ToObject<string>();
		text = text.Replace("[gameId]", args.ServerId.ToString());
		text = text.Replace("[personaRef]", __userContext.UserId.ToString());
		if (args.Role != ZRole.Spectator)
		{
			text = text.Replace("[role]", args.Role.ToString().ToLower());
			text = text.Replace("[isSpectator]", string.Empty);
		}
		else
		{
			text = text.Replace("[role]", ZRole.Soldier.ToString().ToLower());
			text = text.Replace("[isSpectator]", "isspectator=\\\"true\\\"");
		}
		if (!string.IsNullOrWhiteSpace(args.Password))
		{
			string newValue = $"password=\\\"{args.Password}\\\"";
			text = text.Replace("[password]", newValue);
		}
		else
		{
			text = text.Replace("[password]", string.Empty);
		}
		return _createRunGame(zInstalledGame, text, args.Game, architecture);
	}
}
