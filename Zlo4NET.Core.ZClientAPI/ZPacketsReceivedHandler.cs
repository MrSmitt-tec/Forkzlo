using System.Collections.Generic;

namespace Zlo4NET.Core.ZClientAPI;

internal delegate void ZPacketsReceivedHandler(IEnumerable<ZPacket> packets);
