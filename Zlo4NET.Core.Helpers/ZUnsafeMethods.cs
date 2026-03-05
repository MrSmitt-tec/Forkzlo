using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Zlo4NET.Core.Helpers;

internal static class ZUnsafeMethods
{
	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool WaitNamedPipe(string name, int timeout);

	internal static bool NamedPipeExists(string pipeName)
	{
		try
		{
			if (!WaitNamedPipe(Path.GetFullPath("\\\\.\\pipe\\" + pipeName), 0))
			{
				Marshal.GetLastWin32Error();
				return false;
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	[DllImport("user32.dll")]
	internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	internal static extern bool SetForegroundWindow(IntPtr hWnd);
}
