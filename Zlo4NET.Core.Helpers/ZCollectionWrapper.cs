using System.Collections.ObjectModel;
using Zlo4NET.Api.Models.Server;

namespace Zlo4NET.Core.Helpers;

internal class ZCollectionWrapper
{
	public ObservableCollection<ZServerBase> Collection { get; }

	public int Count => ZSynchronizationWrapper.SendReturn((object s) => Collection.Count);

	public ZCollectionWrapper(ObservableCollection<ZServerBase> collection)
	{
		Collection = collection;
	}

	public void Add(ZServerBase item)
	{
		ZSynchronizationWrapper.Send(delegate(ZServerBase s)
		{
			Collection.Add(s);
		}, item);
	}

	public void Remove(ZServerBase item)
	{
		ZSynchronizationWrapper.Send(delegate(ZServerBase s)
		{
			Collection.Remove(s);
		}, item);
	}

	public void Flush()
	{
		ZSynchronizationWrapper.Send<object>(delegate
		{
			Collection.Clear();
		});
	}
}
