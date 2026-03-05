using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using Zlo4NET.Core.Extensions;

namespace Zlo4NET.Core.Data;

internal class _GamePipe
{
	private const int _messageHeaderSize = 4;

	private readonly ZLogger _logger;

	private readonly Thread _readThread;

	private readonly NamedPipeClientStream _pipe;

	private readonly _Buffer _buffer;

	public event _PipeHandler PipeEvent;

	public _GamePipe(ZLogger logger, string pipeName)
	{
		_logger = logger;
		_buffer = new _Buffer();
		_pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
		_readThread = new Thread(_Reader)
		{
			IsBackground = true
		};
	}

	public void Begin()
	{
		_readThread.Start();
	}

	private void _OnPipeEvent(_GameState state)
	{
		this.PipeEvent?.Invoke(state);
	}

	private void _Reader()
	{
		_pipe.Connect();
		while (_pipe.IsConnected && _pipe.CanRead)
		{
			byte[] array = new byte[4096];
			int num = _pipe.Read(array, 0, array.Length);
			if (num > 0)
			{
				IEnumerable<byte> bufferData = array.Take(num);
				_buffer.Append(bufferData);
				_parseData();
			}
			Thread.Sleep(50);
		}
	}

	private void _parseData()
	{
		string rawEvent = string.Empty;
		string text = string.Empty;
		try
		{
			using MemoryStream input = new MemoryStream(_buffer.BufferData, writable: false);
			using BinaryReader binaryReader = new BinaryReader(input, Encoding.ASCII);
			binaryReader.ReadBytes(2);
			ushort num = binaryReader.ReadUInt16();
			if (_buffer.Size < num - 4)
			{
				return;
			}
			byte count = binaryReader.ReadByte();
			rawEvent = binaryReader.ReadCountedString(count).Trim();
			ushort count2 = binaryReader.ReadUInt16();
			text = binaryReader.ReadCountedString(count2).Trim();
			text = Uri.UnescapeDataString(text);
		}
		catch (Exception ex)
		{
			_logger.Error("_parseData message " + ex.Message);
		}
		_buffer.Clear();
		_GameState state = _GameStateParser.ParseStates(rawEvent, text);
		_OnPipeEvent(state);
	}
}
