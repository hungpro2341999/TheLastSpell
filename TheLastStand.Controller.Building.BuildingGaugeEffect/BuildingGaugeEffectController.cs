using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingGaugeEffect;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

public abstract class BuildingGaugeEffectController
{
	public TheLastStand.Model.Building.BuildingGaugeEffect.BuildingGaugeEffect BuildingGaugeEffect { get; protected set; }

	public virtual bool CanTriggerEffect()
	{
		return BuildingGaugeEffect.Units >= BuildingGaugeEffect.UnitsThreshold;
	}

	public virtual List<IEffectTargetSkillActionController> TriggerEffect()
	{
		if (!(BuildingGaugeEffect.ProductionBuilding.BuildingParent is MagicCircle))
		{
			TPSingleton<BarkManager>.Instance.AddPotentialBark("BuildingGaugeCompletion", BuildingGaugeEffect.ProductionBuilding.BuildingParent.BlueprintModule, 0f);
			TPSingleton<BarkManager>.Instance.Display();
		}
		return new List<IEffectTargetSkillActionController>();
	}
}
