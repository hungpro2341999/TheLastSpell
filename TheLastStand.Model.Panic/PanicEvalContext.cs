using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Model.Panic;

public class PanicEvalContext
{
	public int Day => TPSingleton<GameManager>.Instance.Game.DayNumber;
}
