using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Model.CastFx;

public class CastFXInterpreterContext : InterpreterContext
{
	public CastFx CastFx { get; set; }

	protected float Distance => Vector2Int.Distance(CastFx.SourceTile.Position, CastFx.TargetTile.Position);

	public CastFXInterpreterContext(object targetObject)
		: base(targetObject)
	{
		CastFx = targetObject as CastFx;
	}

	protected float Random(double a, double b)
	{
		if (b > a)
		{
			a += b;
			b = a - b;
			a -= b;
		}
		return RandomManager.GetRandomRange(this, (float)a, (float)b);
	}
}
