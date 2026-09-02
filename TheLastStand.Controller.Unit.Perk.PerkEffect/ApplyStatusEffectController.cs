using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ApplyStatusEffectController : APerkEffectController
{
	public ApplyStatusEffect ApplyStatusEffect => base.PerkEffect as ApplyStatusEffect;

	public ApplyStatusEffectController(ApplyStatusEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ApplyStatusEffect(aPerkEffectDefinition as ApplyStatusEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		TPSingleton<PlayableUnitManager>.Instance.ShouldClearUndoStack = true;
		TheLastStand.Model.Status.Status.E_StatusType statusType = ApplyStatusEffect.ApplyStatusEffectDefinition.StatusType;
		PlayableUnit owner = base.PerkEffect.APerkModule.Perk.Owner;
		HashSet<TheLastStand.Model.Unit.Unit> hashSet = new HashSet<TheLastStand.Model.Unit.Unit>();
		TheLastStand.Model.Status.Status status = null;
		hashSet.AddRange(from t in ApplyStatusEffect.PerkTargeting.GetTargetTiles(data, base.PerkEffect.APerkModule.Perk)
			select t.Unit into u
			where u != null
			select u);
		if (hashSet.Count == 0)
		{
			return;
		}
		int num = ApplyStatusEffect.ApplyStatusEffectDefinition.ChanceExpression?.EvalToInt(base.PerkEffect.APerkModule.Perk) ?? 100;
		if (statusType == TheLastStand.Model.Status.Status.E_StatusType.Stun)
		{
			num = (int)(owner.PlayableUnitController.GetModifiedStunChance((float)num / 100f) * 100f);
		}
		int turnsCount = owner.ComputeStatusDuration(statusType, ApplyStatusEffect.ApplyStatusEffectDefinition.TurnsCountExpression.EvalToInt(base.PerkEffect.APerkModule.Perk));
		float value = ApplyStatusEffect.ApplyStatusEffectDefinition.ValueExpression?.EvalToInt(base.PerkEffect.APerkModule.Perk) ?? 1;
		foreach (TheLastStand.Model.Unit.Unit item in hashSet)
		{
			int randomRange = RandomManager.GetRandomRange(this, 0, 100);
			int num2 = ((statusType == TheLastStand.Model.Status.Status.E_StatusType.Stun) ? ((int)(item.UnitController.GetResistedStunChance((float)num / 100f) * 100f)) : num);
			if (randomRange < num2)
			{
				StatusCreationInfo statusCreationInfo = new StatusCreationInfo
				{
					Source = owner,
					Stat = ApplyStatusEffect.ApplyStatusEffectDefinition.Stat,
					TurnsCount = turnsCount,
					Value = value,
					IsFromPerk = true,
					HideDisplayEffect = ApplyStatusEffect.ApplyStatusEffectDefinition.HideDisplayEffect
				};
				TheLastStand.Model.Status.Status status2 = SkillManager.AddStatus(item, statusType, statusCreationInfo, ApplyStatusEffect.ApplyStatusEffectDefinition.RefreshHUD);
				if (ApplyStatusEffect.ApplyStatusEffectDefinition.RefreshHUD)
				{
					item.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: true);
				}
				if (status == null)
				{
					status = status2;
				}
			}
		}
		PerkDataContainer obj = new PerkDataContainer
		{
			Caster = base.PerkEffect.APerkModule.Perk.Owner,
			StatusApplied = status,
			IsTriggeredByPerk = true
		};
		base.PerkEffect.APerkModule.Perk.Owner?.Events.GetValueOrDefault(E_EffectTime.OnPerkApplyStatusEnd)?.Invoke(obj);
	}
}
