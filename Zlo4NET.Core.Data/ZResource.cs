using System.IO;
using Newtonsoft.Json.Linq;

namespace Zlo4NET.Core.Data;

public static class ZResource
{
	private static JToken _getResourceObject(string key, string resource)
	{
		JObject jObject = null;
		using (StreamReader streamReader = new StreamReader(ZInternalResource.GetResourceStream("shared." + resource + ".json")))
		{
			jObject = JObject.Parse(streamReader.ReadToEnd());
		}
		return jObject[key];
	}

	public static string[] GetBF3MapNames()
	{
		return _getResourceObject("bf3", "map").ToObject<string[]>();
	}

	public static string[] GetBF4MapNames()
	{
		return _getResourceObject("bf4", "map").ToObject<string[]>();
	}

	public static string[] GetBFHMapNames()
	{
		return _getResourceObject("bfh", "map").ToObject<string[]>();
	}

	public static string[] GetBF3GameModeNames()
	{
		return _getResourceObject("bf3", "gamemode").ToObject<string[]>();
	}

	public static string[] GetBF4GameModeNames()
	{
		return _getResourceObject("bf4", "gamemode").ToObject<string[]>();
	}

	public static string[] GetBFHGameModeNames()
	{
		return _getResourceObject("bfh", "gamemode").ToObject<string[]>();
	}
}
