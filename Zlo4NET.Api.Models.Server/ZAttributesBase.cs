using System.Collections.Generic;
using Zlo4NET.Core.Data;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Api.Models.Server;

public abstract class ZAttributesBase : ZObservableObject
{
	protected readonly IDictionary<string, string> _attributes;

	[ZObservableProperty]
	public string BannerUrl => _getValue("bannerurl");

	public string Country => _getValue("country");

	[ZObservableProperty]
	public string Message => _getValue("message");

	[ZObservableProperty]
	public string Mod => _getValue("mod");

	[ZObservableProperty]
	public string Preset => _getValue("preset");

	public string PunkBusterVersion => _getValue("punkbusterversion");

	public string Region => _getValue("region");

	[ZObservableProperty]
	public string Description => _getValue("description");

	public string ServerSecret => _getValue("secret");

	public string PunkBuster => _getValue("punkbuster");

	public abstract string FairFight { get; }

	public string ServerType => _getValue("servertype");

	public abstract string TickRate { get; }

	protected ZAttributesBase(IDictionary<string, string> attributes)
	{
		_attributes = attributes;
	}

	protected string _getValue(string key)
	{
		_attributes.TryGetValue(key, out var value);
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		return "NR";
	}
}
