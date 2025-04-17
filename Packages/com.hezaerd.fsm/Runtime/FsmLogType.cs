using System;

namespace hezaerd.fsm
{
	[Flags]
	public enum FsmLogType
	{
		NONE         = 0,
		STATE_ENTER   = 1 << 0,
		STATE_EXIT    = 1 << 1,
		STATE_CREATED = 1 << 2,
		STATE_REGISTERED = 1 << 3,
		ERROR        = 1 << 4,
		ALL          = ~0
	}
}