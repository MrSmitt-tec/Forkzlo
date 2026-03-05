using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Zlo4NET.Api.Models.Shared;

namespace Zlo4NET.Core.Data;

internal class ZMapNameConverter
{
	private readonly JToken _jBF3MapDictionary;

	private readonly JToken _jBF4MapDictionary;

	private readonly JToken _jBFHMapDictionary;

	private JToken _GetJToken(ZGame game)
	{
		return game switch
		{
			ZGame.BF3 => _jBF3MapDictionary, 
			ZGame.BF4 => _jBF4MapDictionary, 
			ZGame.BFH => _jBFHMapDictionary, 
			_ => throw new Exception(), 
		};
	}

	public ZMapNameConverter()
	{
		using StreamReader streamReader = new StreamReader(ZInternalResource.GetResourceStream("maps.json"));
		JObject jObject = JObject.Parse(streamReader.ReadToEnd());
		_jBF3MapDictionary = jObject["bf3"];
		_jBF4MapDictionary = jObject["bf4"];
		_jBFHMapDictionary = jObject["bfh"];
	}

	public string GetMapNameByKey(ZGame game, string mapNameKey)
	{
		try
		{
			return _GetJToken(game)[mapNameKey].ToObject<string>();
		}
		catch (Exception)
		{
			return "NA";
		}
	}
}
