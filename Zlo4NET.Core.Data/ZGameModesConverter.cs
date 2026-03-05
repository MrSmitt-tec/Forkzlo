using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Zlo4NET.Api.Models.Shared;

namespace Zlo4NET.Core.Data;

internal class ZGameModesConverter
{
	private readonly JToken _jBF3GameModeDictionary;

	private readonly JToken _jBF4GameModeDictionary;

	private readonly JToken _jBFHGameModeDictionary;

	private JToken _GetJToken(ZGame game)
	{
		return game switch
		{
			ZGame.BF3 => _jBF3GameModeDictionary, 
			ZGame.BF4 => _jBF4GameModeDictionary, 
			ZGame.BFH => _jBFHGameModeDictionary, 
			_ => throw new Exception(), 
		};
	}

	public ZGameModesConverter()
	{
		using StreamReader streamReader = new StreamReader(ZInternalResource.GetResourceStream("gameModes.json"));
		JObject jObject = JObject.Parse(streamReader.ReadToEnd());
		_jBF3GameModeDictionary = jObject["bf3"];
		_jBF4GameModeDictionary = jObject["bf4"];
		_jBFHGameModeDictionary = jObject["bfh"];
	}

	public string GetGameModeNameByKey(ZGame game, string gameModeKey)
	{
		try
		{
			return _GetJToken(game)[gameModeKey].ToObject<string>();
		}
		catch (Exception)
		{
			return "NA";
		}
	}
}
