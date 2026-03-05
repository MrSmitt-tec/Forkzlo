using System.Collections.Generic;
using System.Linq;
using Zlo4NET.Core.Helpers;

namespace Zlo4NET.Core.ZClientAPI;

internal class ZBuffer
{
	private byte[] _buffer;

	public int Size => _buffer.Length;

	public ZBuffer()
	{
		_buffer = CollectionHelper.GetEmptyEnumerable<byte>().ToArray();
	}

	public ZBuffer(byte[] buffer)
		: this()
	{
		_buffer = buffer;
	}

	public static implicit operator byte[](ZBuffer bufferInstance)
	{
		return bufferInstance?._buffer;
	}

	public void Append(IEnumerable<byte> buffer)
	{
		_buffer = _buffer.Concat(buffer).ToArray();
	}

	public void RemoveBytes(int length)
	{
		_buffer = _buffer.Skip(length).ToArray();
	}

	public void Clear()
	{
		_buffer = CollectionHelper.GetEmptyEnumerable<byte>().ToArray();
	}
}
