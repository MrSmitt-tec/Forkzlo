using System.Collections.Generic;

namespace Zlo4NET.Core.Services;

internal interface IZChangesMapper
{
	void MapChanges<T>(T source, T target);

	void MapCollection<T>(IEnumerable<T> source, ICollection<T> target);
}
