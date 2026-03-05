using System;
using Newtonsoft.Json.Linq;

namespace Zlo4NET.Api.Models.Shared;

public class ZBF3Stats : ZStatsBase
{
	private readonly JObject _stats;

	private readonly JObject _global;

	private readonly JObject _scores;

	private readonly JObject _kits;

	public override byte Rank => _stats["rank"].ToObject<byte>();

	public string RankName => _stats["rankname"].ToObject<string>();

	public float ShortXp => _scores["shortxp"].ToObject<float>();

	public float MaxXp => _scores["maxxp"].ToObject<float>();

	public float Kills => _global["kills"].ToObject<float>();

	public float Deaths => _global["deaths"].ToObject<float>();

	public float Wins => _global["wins"].ToObject<float>();

	public float Losses => _global["losses"].ToObject<float>();

	public float Shots => _global["shots"].ToObject<float>();

	public float Hits => _global["hits"].ToObject<float>();

	public float HeadShots => _global["headshots"].ToObject<float>();

	public float LongestHeadShot => _global["longesths"].ToObject<float>();

	public float VehicleKills => _global["vehiclekills"].ToObject<float>();

	public float Revives => _global["revives"].ToObject<float>();

	public float KillAssists => _global["killassists"].ToObject<float>();

	public float Resupplies => _global["resupplies"].ToObject<float>();

	public float Heals => _global["heals"].ToObject<float>();

	public float Repairs => _global["repairs"].ToObject<float>();

	public float EloGames => _global["elo_games"].ToObject<float>();

	public float KillStreakBonus => _global["killstreakbonus"].ToObject<float>();

	public float VehicleDestroyAssist => _global["vehicledestroyassist"].ToObject<float>();

	public float VehicleDestroyed => _global["vehicledestroyed"].ToObject<float>();

	public float DogTags => _global["dogtags"].ToObject<float>();

	public float AvengerKills => _global["avengerkills"].ToObject<float>();

	public float SaviorKills => _global["saviorkills"].ToObject<float>();

	public float Suppression => _global["suppression"].ToObject<float>();

	public float NemesisStreak => _global["nemesisstreak"].ToObject<float>();

	public float NemesisKills => _global["nemesiskills"].ToObject<float>();

	public float MComDestroyed => _global["mcomdest"].ToObject<float>();

	public float MComDefKills => _global["mcomdefkills"].ToObject<float>();

	public float FlagCaps => _global["flagcaps"].ToObject<float>();

	public float FlagDef => _global["flagdef"].ToObject<float>();

	public float WL => Wins / Losses;

	public float KD => Kills / Deaths;

	public float UntilRankUp => MaxXp - ShortXp;

	public float Accuracy => Hits / Shots;

	public short Time => (short)Math.Floor(_global["time"].ToObject<double>() / 60.0 / 60.0);

	public byte CurrentProgressPercent => (byte)Math.Floor(ShortXp * 100f / MaxXp);

	public byte AssaultStarsCount => _kits["assault"]["star"]["count"].ToObject<byte>();

	public float AssaultScoreMax => _kits["assault"]["star"]["needed"].ToObject<float>();

	public float AssaultCurrentScore => _kits["assault"]["star"]["curr"].ToObject<float>() - AssaultScoreMax * (float)(int)AssaultStarsCount;

	public byte AssaultStarProgressPercent => (byte)Math.Floor(AssaultCurrentScore * 100f / AssaultScoreMax);

	public byte EngineerStarsCount => _kits["engineer"]["star"]["count"].ToObject<byte>();

	public float EngineerScoreMax => _kits["engineer"]["star"]["needed"].ToObject<float>();

	public float EngineerCurrentScore => _kits["engineer"]["star"]["curr"].ToObject<float>() - EngineerScoreMax * (float)(int)EngineerStarsCount;

	public byte EngineerStarProgressPercent => (byte)Math.Floor(EngineerCurrentScore * 100f / EngineerScoreMax);

	public byte ReconStarsCount => _kits["recon"]["star"]["count"].ToObject<byte>();

	public float ReconScoreMax => _kits["recon"]["star"]["needed"].ToObject<float>();

	public float ReconCurrentScore => _kits["recon"]["star"]["curr"].ToObject<float>() - ReconScoreMax * (float)(int)ReconStarsCount;

	public byte ReconStarProgressPercent => (byte)Math.Floor(ReconCurrentScore * 100f / ReconScoreMax);

	public byte SupportStarsCount => _kits["support"]["star"]["count"].ToObject<byte>();

	public float SupportScoreMax => _kits["support"]["star"]["needed"].ToObject<float>();

	public float SupportCurrentScore => _kits["support"]["star"]["curr"].ToObject<float>() - SupportScoreMax * (float)(int)SupportStarsCount;

	public byte SupportStarProgressPercent => (byte)Math.Floor(SupportCurrentScore * 100f / SupportScoreMax);

	public ZBF3Stats(JObject raw)
	{
		if (raw == null)
		{
			throw new ArgumentNullException("raw");
		}
		_stats = (JObject)raw["stats"];
		_global = (JObject)raw["stats"]["global"];
		_scores = (JObject)raw["stats"]["scores"];
		_kits = (JObject)raw["stats"]["kits"];
	}
}
