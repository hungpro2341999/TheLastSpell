using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Model.Trophy;

public class MultiplierEvalContext
{
	public int Night => TPSingleton<GameManager>.Instance.Game.DayNumber;
}
