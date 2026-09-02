using System;

namespace TheLastStand.View.HUD;

[Flags]
public enum E_GamepadButtonType
{
	NONE = 0,
	A = 1,
	Y = 2,
	B = 4,
	X = 8,
	RB = 0x10,
	LB = 0x20,
	RT = 0x40,
	LT = 0x80,
	DPAD_BOT = 0x100,
	DPAD_TOP = 0x200,
	DPAD_LEFT = 0x400,
	DPAD_RIGHT = 0x800,
	SELECT = 0x1000,
	START = 0x2000
}
