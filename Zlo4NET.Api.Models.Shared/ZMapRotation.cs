using System.Collections.ObjectModel;
using Zlo4NET.Api.Models.Server;
using Zlo4NET.Core.Data;
using Zlo4NET.Core.Data.Attributes;

namespace Zlo4NET.Api.Models.Shared;

public class ZMapRotation : ZObservableObject
{
	private int[] _rotationIndexes;

	[ZObservableProperty]
	[ZMapperProperty]
	public ZMap Current { get; set; }

	[ZObservableProperty]
	[ZMapperProperty]
	public ZMap Next { get; set; }

	public ObservableCollection<ZMap> Rotation { get; set; }

	public ZMapRotation(int[] rotation)
	{
		_rotationIndexes = rotation;
	}
}
