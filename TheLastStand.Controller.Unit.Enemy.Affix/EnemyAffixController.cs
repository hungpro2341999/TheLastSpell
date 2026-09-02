using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public abstract class EnemyAffixController
{
	public EnemyAffix EnemyAffix { get; protected set; }

	public abstract void Trigger(E_EffectTime effectTime, object data = null);
}
