using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Data;
using Zlo4NET.Core.Extensions;

namespace Zlo4NET.Core.ZClientAPI;

internal class ZClientImpl : IZClient
{
	private class SocketAsyncState
	{
		public Socket WorkSocket { get; }

		public byte[] Buffer { get; }

		public long BufferSize => Buffer.Length;

		public SocketAsyncState(Socket workSocket)
		{
			WorkSocket = workSocket;
		}

		public SocketAsyncState(Socket workSocket, byte[] buffer)
			: this(workSocket)
		{
			Buffer = buffer;
		}
	}

	private const int BUFFER_SIZE = 8192;

	private const int HEADER_SIZE = 5;

	private readonly LingerOption _lingerOption;

	private readonly IPEndPoint _endPoint;

	private readonly ZBuffer _buffer;

	private readonly ZLogger _logger;

	private Socket _currentSocket;

	private bool _socketCloseInitiated;

	public event Action<bool> ConnectionStateChanged;

	public event ZPacketsReceivedHandler PacketsReceived;

	public ZClientImpl()
	{
		_lingerOption = new LingerOption(enable: false, 0);
		_endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 48486);
		_buffer = new ZBuffer();
		_logger = ZLogger.Instance;
	}

	private Socket _createSocket()
	{
		return new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
		{
			LingerState = _lingerOption
		};
	}

	private IAsyncResult _socketBeginConnect(Socket workSocket)
	{
		return _currentSocket.BeginConnect(_endPoint, _EndConnectCallback, new SocketAsyncState(workSocket));
	}

	private IAsyncResult _socketBeginReceive(Socket workSocket, byte[] buffer)
	{
		return _currentSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, _EndReceiveCallback, new SocketAsyncState(workSocket, buffer));
	}

	private IAsyncResult _socketBeginSend(Socket workSocket, byte[] buffer)
	{
		return _currentSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, _EndSendCallback, new SocketAsyncState(workSocket, buffer));
	}

	private void _closeSocket(Socket workSocket)
	{
		try
		{
			workSocket.Shutdown(SocketShutdown.Both);
		}
		catch
		{
		}
		finally
		{
			workSocket.Close();
			_buffer.Clear();
			_socketCloseInitiated = false;
		}
	}

	private void _EndConnectCallback(IAsyncResult asyncResult)
	{
		Socket workSocket = ((SocketAsyncState)asyncResult.AsyncState).WorkSocket;
		try
		{
			workSocket.EndConnect(asyncResult);
			if (workSocket.Connected)
			{
				byte[] buffer = new byte[8192];
				_socketBeginReceive(workSocket, buffer);
			}
		}
		catch (SocketException ex)
		{
			_logSocketMessage(ZLogLevel.Error, $"Socket connection error {ex.SocketErrorCode} {ex.ErrorCode}", "_EndConnectCallback");
			_closeSocket(workSocket);
		}
		catch (ObjectDisposedException)
		{
		}
		catch (Exception ex3)
		{
			_logSocketMessage(ZLogLevel.Error, "Socket unexpected error " + ex3.Message, "_EndConnectCallback");
			_closeSocket(workSocket);
		}
		finally
		{
			if (_currentSocket == workSocket)
			{
				_OnConnectionStateChanged(workSocket.Connected);
			}
		}
	}

	private void _EndReceiveCallback(IAsyncResult asyncResult)
	{
		SocketAsyncState socketAsyncState = (SocketAsyncState)asyncResult.AsyncState;
		Socket workSocket = socketAsyncState.WorkSocket;
		try
		{
			int num = workSocket.EndReceive(asyncResult);
			if (num == 0)
			{
				throw new Exception("The sender has closed their connection");
			}
			byte[] buffer = socketAsyncState.Buffer;
			IEnumerable<byte> buffer2 = buffer.Take(num);
			_buffer.Append(buffer2);
			_OnBytesReceived();
			if (workSocket.Connected)
			{
				_socketBeginReceive(workSocket, buffer);
			}
		}
		catch (SocketException ex)
		{
			_logSocketMessage(ZLogLevel.Error, $"Socket receive error {ex.SocketErrorCode} {ex.ErrorCode}", "_EndReceiveCallback");
			_closeSocket(workSocket);
		}
		catch (ObjectDisposedException)
		{
		}
		catch (Exception ex3)
		{
			_logSocketMessage(ZLogLevel.Error, "Socket unexpected error " + ex3.Message, "_EndReceiveCallback");
			_closeSocket(workSocket);
		}
		finally
		{
			if (_currentSocket == workSocket && !workSocket.Connected)
			{
				_OnConnectionStateChanged(connectionState: false);
			}
		}
	}

	private void _EndSendCallback(IAsyncResult asyncResult)
	{
		SocketAsyncState socketAsyncState = (SocketAsyncState)asyncResult.AsyncState;
		Socket workSocket = socketAsyncState.WorkSocket;
		try
		{
			if (workSocket.EndSend(asyncResult) != socketAsyncState.BufferSize)
			{
				throw new Exception("The sender has closed their connection");
			}
		}
		catch (SocketException ex)
		{
			_logSocketMessage(ZLogLevel.Error, $"Socket send error {ex.ErrorCode} {ex.SocketErrorCode}", "_EndSendCallback");
			_closeSocket(workSocket);
		}
		catch (ObjectDisposedException)
		{
		}
		catch (Exception ex3)
		{
			_logSocketMessage(ZLogLevel.Error, "Socket unexpected error " + ex3.Message, "_EndSendCallback");
			_closeSocket(workSocket);
		}
		finally
		{
			if (_currentSocket == workSocket && !workSocket.Connected)
			{
				_OnConnectionStateChanged(connectionState: false);
			}
		}
	}

	private void _OnBytesReceived()
	{
		List<ZPacket> list = new List<ZPacket>(1);
		using (MemoryStream input = new MemoryStream(_buffer, writable: false))
		{
			using BinaryReader binaryReader = new BinaryReader(input, Encoding.ASCII);
			while (binaryReader.PeekChar() != -1 && binaryReader.BytesRemaining() >= 5)
			{
				ZCommand id = (ZCommand)binaryReader.ReadByte();
				int num = (int)binaryReader.ReadZUInt32();
				if (num <= binaryReader.BytesRemaining())
				{
					byte[] array = new byte[num];
					binaryReader.Read(array, 0, num);
					list.Add(new ZPacket
					{
						Id = id,
						Payload = array
					});
					_buffer.RemoveBytes(5 + num);
					continue;
				}
				break;
			}
		}
		if (list.Count != 0)
		{
			_OnPacketsReceived(list);
		}
	}

	private void _OnConnectionStateChanged(bool connectionState)
	{
		this.ConnectionStateChanged?.Invoke(connectionState);
	}

	private void _OnPacketsReceived(IEnumerable<ZPacket> packets)
	{
		this.PacketsReceived?.Invoke(packets);
	}

	private void _logSocketMessage(ZLogLevel level, string message, [CallerMemberName] string callerName = null)
	{
		if (!_socketCloseInitiated)
		{
			_logger.Log(level, callerName + " " + message);
		}
	}

	public void Run()
	{
		if (_currentSocket != null)
		{
			Close();
		}
		_currentSocket = _createSocket();
		_socketBeginConnect(_currentSocket);
	}

	public void Close()
	{
		_socketCloseInitiated = true;
		_closeSocket(_currentSocket);
	}

	public void SendRequest(byte[] requestBytes)
	{
		_socketBeginSend(_currentSocket, requestBytes);
	}
}
