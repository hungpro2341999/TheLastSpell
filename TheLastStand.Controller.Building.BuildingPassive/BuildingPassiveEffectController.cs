using TheLastStand.Model.Building.BuildingPassive;

namespace TheLastStand.Controller.Building.BuildingPassive;

public abstract class BuildingPassiveEffectController
{
	public BuildingPassiveEffect BuildingPassiveEffect { get; protected set; }

	public abstract void Apply();

	public virtual void ImproveEffect(int bonus)
	{
	}

	public virtual void Unapply()
	{
	}

	public virtual void OnDeath()
	{
	}
}
