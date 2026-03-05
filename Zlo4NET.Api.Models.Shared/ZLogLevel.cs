using System;

namespace Zlo4NET.Api.Models.Shared;

[Flags]
public enum ZLogLevel
{
	Info = 2,
	Debug = 4,
	Warning = 8,
	Error = 0x10
}
