using System;
using System.Threading;
using Zlo4NET.Api.Models.Shared;
using Zlo4NET.Core.Data;

namespace Zlo4NET.Core.Helpers;

internal static class ZSynchronizationWrapper
{
	private static SynchronizationContext _context;

	public static void Initialize(ZConfiguration config)
	{
		_context = config.SynchronizationContext;
	}

	internal static void Post<T>(Action<T> action, T state = default(T))
	{
		_context.Post(delegate(object s)
		{
			action((T)s);
		}, state);
	}

	internal static void Send<T>(Action<T> action, T state = default(T))
	{
		_context.Send(delegate(object s)
		{
			action((T)s);
		}, state);
	}

	internal static TResult SendReturn<TResult, TState>(Func<TState, TResult> action, TState state = default(TState))
	{
		ZActionState<TResult, TState> zActionState = new ZActionState<TResult, TState>
		{
			State = state
		};
		_context.Send(delegate(object s)
		{
			ZActionState<TResult, TState> zActionState2 = (ZActionState<TResult, TState>)s;
			zActionState2.Result = action(zActionState2.State);
		}, zActionState);
		return zActionState.Result;
	}
}
