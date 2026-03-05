using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zlo4NET.Core.Data;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Core.Helpers;

internal static class ZObservableHelper
{
	public static IEnumerable<PropertyInfo> GetObservablePropertiesFromType(IReflect reflectType)
	{
		return from propertyInfo in reflectType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			where propertyInfo.GetCustomAttributes(typeof(ZObservablePropertyAttribute), inherit: false).Any()
			select propertyInfo;
	}

	public static IEnumerable<PropertyInfo> GetObservableObjectPropertiesFromType(IReflect reflectType)
	{
		return from propertyInfo in reflectType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			where propertyInfo.PropertyType.IsSubclassOf(typeof(ZObservableObject))
			select propertyInfo;
	}
}
