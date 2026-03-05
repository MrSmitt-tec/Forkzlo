using System.IO;
using System.Reflection;

namespace Zlo4NET.Core.Data;

public static class ZInternalResource
{
	public static Stream GetResourceStream(string internalPath)
	{
		return Assembly.GetExecutingAssembly().GetManifestResourceStream("Zlo4NET.Resources." + internalPath);
	}
}
