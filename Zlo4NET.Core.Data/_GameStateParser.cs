using System;
using System.Collections.Generic;
using System.Linq;
using Zlo4NET.Core.Helpers;

namespace Zlo4NET.Core.Data;

internal static class _GameStateParser
{
	private static readonly IReadOnlyDictionary<string, ZGameState> _states = new Dictionary<string, ZGameState>
	{
		{
			"",
			ZGameState.State_Empty
		},
		{
			"State_GameRun",
			ZGameState.State_GameRun
		},
		{
			"State_GameClose",
			ZGameState.State_GameClose
		},
		{
			"State_NA",
			ZGameState.State_NA
		},
		{
			"State_Error",
			ZGameState.State_Error
		},
		{
			"State_Starting",
			ZGameState.State_Starting
		},
		{
			"State_Init",
			ZGameState.State_Init
		},
		{
			"State_NotLoggedIn",
			ZGameState.State_NotLoggedIn
		},
		{
			"State_MenuReady",
			ZGameState.State_MenuReady
		},
		{
			"State_Matchmaking",
			ZGameState.State_Matchmaking
		},
		{
			"State_MatchmakeResultHost",
			ZGameState.State_MatchmakeResultHost
		},
		{
			"State_MatchmakeResultJoin",
			ZGameState.State_MatchmakeResultJoin
		},
		{
			"State_ConnectToGameId",
			ZGameState.State_ConnectToGameId
		},
		{
			"State_ConnectToUserId",
			ZGameState.State_ConnectToUserId
		},
		{
			"State_CreateCoOpPeer",
			ZGameState.State_CreateCoOpPeer
		},
		{
			"State_MatchmakeCoOp",
			ZGameState.State_MatchmakeCoOp
		},
		{
			"State_ResumeCampaign",
			ZGameState.State_ResumeCampaign
		},
		{
			"State_LaunchPlayground",
			ZGameState.State_LaunchPlayground
		},
		{
			"State_WeaponCustomization",
			ZGameState.State_WeaponCustomization
		},
		{
			"State_LoadLevel",
			ZGameState.State_LoadLevel
		},
		{
			"State_Connecting",
			ZGameState.State_Connecting
		},
		{
			"State_WaitForLevel",
			ZGameState.State_WaitForLevel
		},
		{
			"State_GameLoading",
			ZGameState.State_GameLoading
		},
		{
			"State_Game",
			ZGameState.State_Game
		},
		{
			"State_GameLeaving",
			ZGameState.State_GameLeaving
		},
		{
			"State_InQueue",
			ZGameState.State_InQueue
		},
		{
			"State_WaitForPeerClient",
			ZGameState.State_WaitForPeerClient
		},
		{
			"State_PeerClientConnected",
			ZGameState.State_PeerClientConnected
		},
		{
			"State_ClaimReservation",
			ZGameState.State_ClaimReservation
		},
		{
			"State_Ready",
			ZGameState.State_Ready
		},
		{
			"State_Sparta",
			ZGameState.State_Sparta
		},
		{
			"State_ShutdownStateMachine",
			ZGameState.State_ShutdownStateMachine
		}
	};

	private static readonly IReadOnlyDictionary<string, ZGameEvent> _events = new Dictionary<string, ZGameEvent>
	{
		{
			"GameWaiting",
			ZGameEvent.GameWaiting
		},
		{
			"StateChanged",
			ZGameEvent.StateChanged
		},
		{
			"Alert",
			ZGameEvent.Alert
		}
	};

	private static readonly ZLogger _log = ZLogger.Instance;

	public static _GameState ParseStates(string rawEvent, string rawState)
	{
		_events.TryGetValue(rawEvent, out var value);
		_GameState gameState = new _GameState
		{
			RawEvent = rawEvent,
			RawState = rawState,
			Event = value,
			States = CollectionHelper.GetEmptyEnumerable<ZGameState>().ToArray()
		};
		switch (value)
		{
		case ZGameEvent.StateChanged:
		{
			int num = rawState.IndexOfAny("0123456789".ToCharArray());
			num = ((num != -1) ? num : rawState.Length);
			ZGameState[] states = rawState.Substring(0, num).Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(delegate(string state)
			{
				_states.TryGetValue(state, out var value2);
				return value2;
			})
				.ToArray();
			gameState.States = states;
			break;
		}
		default:
			_log.Warning("_GameStateParser event doesn't match (" + rawEvent + " " + rawState + ")");
			break;
		case ZGameEvent.Alert:
		case ZGameEvent.GameWaiting:
			break;
		}
		return gameState;
	}
}
