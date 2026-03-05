using System.Collections.Generic;
using System.Reflection;
using Zlo4NET.Core.Helpers;
using Zlo4NET.Core.Services;

namespace Zlo4NET.Core.Data;

internal class ZChangesMapper : IZChangesMapper
{
	public void MapChanges<T>(T source, T target)
	{
		foreach (PropertyInfo item in ZMapperHelper.GetMapperPropertiesFromType(typeof(T)))
		{
			object value = item.GetValue(source);
			object value2 = item.GetValue(target);
			if (value == null || !value.Equals(value2))
			{
				item.SetValue(target, value);
			}
		}
	}

	public void MapCollection<T>(IEnumerable<T> source, ICollection<T> target)
	{
		target.Clear();
		foreach (T item in source)
		{
			target.Add(item);
		}
	}
}
