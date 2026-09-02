using TPLib;

namespace TheLastStand.Manager.Trap;

public class TrapSoundSkillDelayInterpreterContext
{
	protected float Random(double a, double b)
	{
		if (b > a)
		{
			a += b;
			b = a - b;
			a -= b;
		}
		return RandomManager.GetRandomRange(TPSingleton<TrapManager>.Instance, (float)a, (float)b);
	}
}
