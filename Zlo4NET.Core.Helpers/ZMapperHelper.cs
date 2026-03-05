using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Core.Helpers;

internal static class ZMapperHelper
{
	public static IEnumerable<PropertyInfo> GetMapperPropertiesFromType(IReflect reflectType)
	{
		return from propertyInfo in reflectType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			where propertyInfo.GetCustomAttributes(typeof(ZMapperPropertyAttribute), inherit: false).Any()
			select propertyInfo;
	}
}
