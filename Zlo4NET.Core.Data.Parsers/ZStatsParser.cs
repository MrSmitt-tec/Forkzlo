using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Extensions;
using Zlo4NET.Core.Services;
using Zlo4NET.Core.ZClientAPI;

namespace Zlo4NET.Core.Data.Parsers;

internal class ZStatsParser : IZStatsParser
{
	private readonly JObject _statsTemplates;

	private static string[] ranknames { get; } = new string[46]
	{
		"Noob", "Private First Class", "Private First Class 1 Star", "Private First Class 2 Stars", "Private First Class 3 Stars", "Lance Corporal", "Lance Corporal 1 Star", "Lance Corporal 2 Stars", "Lance Corporal 3 Stars", "Corporal",
		"Corporal 1 Star", "Corporal 2 Stars", "Corporal 3 Stars", "Sergeant", "Sergeant 1 Star", "Sergeant 2 Stars", "Sergeant 3 Stars", "Staff Sergeant", "Staff Sergeant 1 Star", "Staff Sergeant 2 Stars",
		"Gunnery Sergeant", "Gunnery Sergeant 1 Star", "Gunnery Sergeant 2 Stars", "Master Sergeant", "Master Sergeant 1 Star", "Master Sergeant 2 Stars", "First Sergeant", "First Sergeant 1 Star", "First Sergeant 2 Stars", "Master Gunnery Sergeant",
		"Master Gunnery Sergeant 1 Star", "Master Gunnery Sergeant 2 Stars", "Sergeant Major", "Sergeant Major 1 Star", "Sergeant Major 2 Star", "Warrant Officer 1", "Chief Warrant Officer 2", "Chief Warrant Officer 3", "Chief Warrant Officer 4", "Chief Warrant Officer 5",
		"Second Lieutenant", "First Lieutenant", "Captain", "Major", "Lt. Colonel", "Colonel"
	};

	private static int[] maxranks { get; } = new int[46]
	{
		1000, 7000, 10000, 11000, 12000, 13000, 13000, 14000, 15000, 15000,
		19000, 20000, 20000, 20000, 30000, 30000, 30000, 30000, 30000, 30000,
		30000, 30000, 40000, 40000, 40000, 40000, 40000, 40000, 40000, 50000,
		50000, 50000, 50000, 50000, 50000, 50000, 50000, 55000, 55000, 60000,
		60000, 60000, 60000, 60000, 80000, 230000
	};

	public ZStatsParser()
	{
		_statsTemplates = _LoadResourceByName("stats.template.json");
	}

	public ZBF3Stats ParseBF3Stats(ZPacket[] packets)
	{
		ZPacket packet = packets.First();
		IDictionary<string, float> statsDictionary = _parseStatsDictionary(packet);
		JObject jObject = _statsTemplates["bf3"] as JObject;
		_assign(statsDictionary, jObject);
		JToken? jToken = jObject["stats"];
		int num = jToken.Value<int>("rank");
		jToken["rankname"] = GetBF3RankName(num);
		JToken jToken2 = jToken["scores"];
		double num2 = SumIfNum(jToken2["vehicleaa"], jToken2["vehicleah"], jToken2["vehicleifv"], jToken2["vehiclejet"], jToken2["vehiclembt"], jToken2["vehiclesh"], jToken2["vehiclelbt"], jToken2["vehicleart"]);
		double num3 = SumIfNum(jToken2["support"], jToken2["assault"], jToken2["engineer"], jToken2["recon"]) + num2 + SumIfNum(jToken2["unlock"], jToken2["award"], jToken2["special"]);
		jToken2["maxxp"] = GetRankMaxScore(num);
		jToken2["shortxp"] = num3 - Sumfrom0to(num);
		jToken2["longxp"] = num3;
		return new ZBF3Stats(jObject);
	}

	public ZBF4Stats ParseBF4Stats(ZPacket[] packets)
	{
		ZPacket packet = packets.First();
		IDictionary<string, float> statsDictionary = _parseStatsDictionary(packet);
		JObject jObject = _statsTemplates["bf4"] as JObject;
		JObject jObject2 = _LoadResourceByName("stats.bf4_details.json");
		_assign(statsDictionary, jObject);
		long num = jObject["player"]["score"].ToObject<long>();
		JObject jObject3 = jObject["player"]["rank"] as JObject;
		int num2 = jObject3.Value<int>("nr");
		JObject jObject4 = jObject2[num2.ToString()] as JObject;
		JObject obj = jObject2[(num2 + 1).ToString()] as JObject;
		long num3 = jObject4["XP Min Total"].ToObject<long>();
		long num4 = num - num3;
		long num5 = obj["XP Min Relative"].ToObject<long>();
		jObject3["name"] = jObject4["Rank Title"];
		jObject3["Unlocks"] = jObject4["Unlocks"];
		jObject3["Short XP"] = num4;
		jObject3["Long XP"] = num;
		jObject3["needed"] = num5 - num4;
		jObject3["Max XP"] = num5;
		foreach (JProperty item in ((JObject)jObject["stats"]["kits"]).Properties())
		{
			JObject obj2 = item.Value as JObject;
			double num6 = obj2["score"].ToObject<double>();
			double num7 = obj2["stars"]["Max"].ToObject<double>();
			double num8 = num6 / num7;
			obj2["stars"]["count"] = (int)num8;
			double num9 = num6 - (double)(int)num8 * num7;
			obj2["stars"]["shortCurr"] = num9;
			obj2["stars"]["progress"] = num9 / num7 * 100.0;
		}
		return new ZBF4Stats(jObject);
	}

	private JObject _LoadResourceByName(string name)
	{
		using StreamReader streamReader = new StreamReader(ZInternalResource.GetResourceStream(name));
		return JObject.Parse(streamReader.ReadToEnd());
	}

	private IDictionary<string, float> _parseStatsDictionary(ZPacket packet)
	{
		using MemoryStream input = new MemoryStream(packet.Payload, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input, Encoding.ASCII);
		binaryReader.SkipBytes(1);
		ushort num = binaryReader.ReadZUInt16();
		IDictionary<string, float> dictionary = new Dictionary<string, float>(num);
		for (ushort num2 = 0; num2 < num; num2++)
		{
			string key = binaryReader.ReadZString();
			float value = binaryReader.ReadZFloat();
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	private void _assign(IDictionary<string, float> statsDictionary, JObject jToken)
	{
		foreach (KeyValuePair<string, JToken> item in jToken)
		{
			if (item.Value.Type == JTokenType.String)
			{
				string text = item.Value.ToObject<string>();
				if (text.StartsWith("stat."))
				{
					statsDictionary.TryGetValue(text.Substring(5), out var value);
					jToken[item.Key] = value;
				}
			}
			else if (item.Value.Type == JTokenType.Object)
			{
				_assign(statsDictionary, (JObject)item.Value);
			}
			else
			{
				if (item.Value.Type != JTokenType.Array)
				{
					continue;
				}
				foreach (JToken item2 in (JArray)item.Value)
				{
					if (item2.Type == JTokenType.Object)
					{
						_assign(statsDictionary, (JObject)item2);
					}
				}
			}
		}
	}

	private double Sumfrom0to(int index)
	{
		float num = 0f;
		for (int i = 0; i < index; i++)
		{
			num += (float)GetRankMaxScore(i);
		}
		return num;
	}

	public static string GetBF3RankName(int rank)
	{
		if (rank <= 45)
		{
			return ranknames[rank];
		}
		return $"Colonel Service Star {rank - 45}";
	}

	public static int GetRankMaxScore(int rank)
	{
		if (rank <= 45)
		{
			return maxranks[rank];
		}
		if (rank == 145)
		{
			return 0;
		}
		return 230000;
	}

	public static double SumIfNum(params JToken[] objects)
	{
		double num = 0.0;
		foreach (JToken jToken in objects)
		{
			if (IsNum(jToken))
			{
				num += (double)jToken;
			}
		}
		return num;
	}

	public static bool IsNum(object obj)
	{
		double result;
		return double.TryParse(obj.ToString(), out result);
	}
}
