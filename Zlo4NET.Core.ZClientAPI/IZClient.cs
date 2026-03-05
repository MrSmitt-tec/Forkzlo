using System;

namespace Zlo4NET.Core.ZClientAPI;

internal interface IZClient
{
	event Action<bool> ConnectionStateChanged;

	event ZPacketsReceivedHandler PacketsReceived;

	void Run();

	void Close();

	void SendRequest(byte[] requestBytes);
}
