using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zlo4NET.Core.ZClientAPI;

internal static class ZRouter
{
	private class ZRequestMetadata
	{
		public TaskCompletionSource<object> TaskCompletionSource { get; }

		public ZRequest Request { get; }

		public ZResponse Response { get; }

		public Guid RequestGuid => Request.RequestGuid;

		public ZRequestMetadata(ZRequest request)
		{
			Request = request;
			Response = new ZResponse(request)
			{
				StatusCode = ZResponseStatusCode.Ok
			};
			TaskCompletionSource = ((request.Method == ZRequestMethod.Get) ? new TaskCompletionSource<object>() : null);
		}
	}

	private class ZStreamMetadata
	{
		public ZCommand StreamCommand { get; }

		public ZPacketsStreamCallback OnPacketsReceivedCallback { get; }

		public ZStreamRejectedCallback StreamRejectedCallback { get; }

		public bool IsRejected { get; set; }

		public ZStreamMetadata(ZCommand streamCommand, ZPacketsStreamCallback packetsReceivedCallback, ZStreamRejectedCallback streamRejectedCallback)
		{
			StreamCommand = streamCommand;
			OnPacketsReceivedCallback = packetsReceivedCallback;
			StreamRejectedCallback = streamRejectedCallback;
		}
	}

	private const int RQ_TIMEOUT = 7000;

	private const ZResponseStatusCode RQ_DEFAULT_STATUS = ZResponseStatusCode.Ok;

	private static IZClient _client;

	private static IList<ZRequestMetadata> _requestsPool;

	private static IList<ZStreamMetadata> _streamsPool;

	private static bool _internalConnectionState;

	public static event Action<bool> ConnectionChanged;

	static ZRouter()
	{
		_internalConnectionState = false;
	}

	public static void Initialize()
	{
		_client = new ZClientImpl();
		_requestsPool = new List<ZRequestMetadata>();
		_streamsPool = new List<ZStreamMetadata>();
		_client.ConnectionStateChanged += _ClientOnConnectionChangedCallback;
		_client.PacketsReceived += _ClientOnPacketsReceivedCallback;
	}

	public static void Start()
	{
		_client.Run();
	}

	public static void Stop()
	{
		_client.Close();
	}

	public static async Task<ZResponse> GetResponseAsync(ZRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (_requestsPool.Any((ZRequestMetadata i) => i.RequestGuid == request.RequestGuid))
		{
			throw new ArgumentException($"This request is already exists in requests pool. {request}");
		}
		return await _RegisterRequestAndWaitResponseAsync(request);
	}

	public static async Task<ZResponse> OpenStreamAsync(ZRequest request, ZPacketsStreamCallback onPacketsReceivedCallback, ZStreamRejectedCallback streamRejectedCallback = null)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (onPacketsReceivedCallback == null)
		{
			throw new ArgumentNullException("onPacketsReceivedCallback");
		}
		if (_streamsPool.Any((ZStreamMetadata i) => i.StreamCommand == request.RequestCommand))
		{
			throw new ArgumentException($"This stream request is already exists in streams pool. {request}");
		}
		return await _TryRegisterStreamAndWaitResponseAsync(request, onPacketsReceivedCallback, streamRejectedCallback);
	}

	public static async Task<ZResponse> CloseStreamAsync(ZRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (_streamsPool.All((ZStreamMetadata i) => i.StreamCommand != request.RequestCommand))
		{
			throw new ArgumentException($"This stream request is doesn't exists in streams pool. {request}");
		}
		return await _TryCloseStreamAndWaitResponseAsync(request);
	}

	private static async Task<ZResponse> _TryCloseStreamAndWaitResponseAsync(ZRequest request)
	{
		ZStreamMetadata streamMetadata = _streamsPool.First((ZStreamMetadata i) => i.StreamCommand == request.RequestCommand);
		ZResponse result = ((!streamMetadata.IsRejected) ? (await _RegisterRequestAndWaitResponseAsync(request)) : new ZResponse(request)
		{
			StatusCode = ZResponseStatusCode.Rejected
		});
		_streamsPool.Remove(streamMetadata);
		return result;
	}

	private static async Task<ZResponse> _TryRegisterStreamAndWaitResponseAsync(ZRequest request, ZPacketsStreamCallback onPacketsReceivedCallback, ZStreamRejectedCallback streamRejectedCallback)
	{
		ZStreamMetadata streamMetadata = new ZStreamMetadata(request.RequestCommand, onPacketsReceivedCallback, streamRejectedCallback);
		_streamsPool.Add(streamMetadata);
		ZResponse obj = await _RegisterRequestAndWaitResponseAsync(request);
		if (obj.StatusCode != ZResponseStatusCode.Ok)
		{
			_streamsPool.Remove(streamMetadata);
		}
		return obj;
	}

	private static async Task<ZResponse> _RegisterRequestAndWaitResponseAsync(ZRequest request)
	{
		ZRequestMetadata requestMetadata = new ZRequestMetadata(request);
		_requestsPool.Add(requestMetadata);
		if (_internalConnectionState)
		{
			_SendRequest(request);
			await _WaitResponseAsync(request, requestMetadata);
		}
		else
		{
			requestMetadata.Response.StatusCode = ZResponseStatusCode.Declined;
		}
		_requestsPool.Remove(requestMetadata);
		return requestMetadata.Response;
	}

	private static void _SendRequest(ZRequest request)
	{
		byte[] requestBytes = request.ToByteArray();
		_client.SendRequest(requestBytes);
	}

	private static async Task _WaitResponseAsync(ZRequest request, ZRequestMetadata metadata)
	{
		if (request.Method == ZRequestMethod.Get)
		{
			Task timeoutTask = Task.Delay(7000);
			Task task = await Task.WhenAny(metadata.TaskCompletionSource.Task, timeoutTask);
			if (task == timeoutTask || metadata.Response.StatusCode == ZResponseStatusCode.Ok)
			{
				metadata.Response.StatusCode = ((task == timeoutTask) ? ZResponseStatusCode.Timeout : ZResponseStatusCode.Ok);
			}
		}
		else
		{
			metadata.Response.StatusCode = ZResponseStatusCode.Ok;
		}
	}

	private static void _OnConnectionChanged(bool connectionState)
	{
		ZRouter.ConnectionChanged?.Invoke(connectionState);
	}

	private static void _ClientOnConnectionChangedCallback(bool connectionState)
	{
		_internalConnectionState = connectionState;
		if (!_internalConnectionState)
		{
			List<ZRequestMetadata> list = _requestsPool.ToList();
			List<ZStreamMetadata> list2 = _streamsPool.ToList();
			foreach (ZRequestMetadata item in list)
			{
				item.Response.StatusCode = ZResponseStatusCode.Rejected;
				item.TaskCompletionSource.SetResult(null);
			}
			foreach (ZStreamMetadata streamMetadata in list2)
			{
				streamMetadata.IsRejected = true;
				streamMetadata.StreamRejectedCallback?.BeginInvoke(delegate(IAsyncResult ar)
				{
					streamMetadata.StreamRejectedCallback.EndInvoke(ar);
				}, null);
			}
		}
		_OnConnectionChanged(_internalConnectionState);
	}

	private static void _ClientOnPacketsReceivedCallback(IEnumerable<ZPacket> packets)
	{
		foreach (IGrouping<ZCommand, ZPacket> packetGroup in from i in packets
			group i by i.Id)
		{
			ZPacket[] array = packetGroup.ToArray();
			ZStreamMetadata streamMetadata = _streamsPool.FirstOrDefault((ZStreamMetadata i) => i.StreamCommand == packetGroup.Key);
			if (streamMetadata != null)
			{
				streamMetadata.OnPacketsReceivedCallback.BeginInvoke(array, delegate(IAsyncResult ar)
				{
					streamMetadata.OnPacketsReceivedCallback.EndInvoke(ar);
				}, null);
				continue;
			}
			ZRequestMetadata zRequestMetadata = _requestsPool.FirstOrDefault((ZRequestMetadata i) => i.Request.RequestCommand == packetGroup.Key);
			if (zRequestMetadata != null)
			{
				zRequestMetadata.Response.ResponsePackets = array;
				zRequestMetadata.Response.StatusCode = ZResponseStatusCode.Ok;
				zRequestMetadata.TaskCompletionSource.SetResult(null);
			}
		}
	}
}
