using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.Controller.Status;

public class ContagionStatusController : StatusController
{
	public ContagionStatus ContagionStatus => base.Status as ContagionStatus;

	public ContagionStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new ContagionStatus(this, unit, statusCreationInfo);
	}

	public void ApplyContagion()
	{
		List<Tile> list = base.Status.Unit.UnitController.GetAdjacentTiles().Where(delegate(Tile o)
		{
			if (!o.HasFog)
			{
				TheLastStand.Model.Unit.Unit unit2 = o.Unit;
				if (unit2 != null && unit2.CanBeDamaged())
				{
					TheLastStand.Model.Unit.Unit unit3 = o.Unit;
					if (unit3 != null && !unit3.IsDeadOrDeathRattling)
					{
						TheLastStand.Model.Unit.Unit unit4 = o.Unit;
						if (unit4 != null && !unit4.IsContagious)
						{
							TheLastStand.Model.Unit.Unit unit5 = o.Unit;
							if (unit5 != null && !unit5.IsImmune)
							{
								return base.Status.Unit.IsAlly(o.Unit);
							}
						}
					}
				}
			}
			return false;
		}).ToList();
		if (list.Count == 0)
		{
			return;
		}
		list = RandomManager.Shuffle(TPSingleton<EffectManager>.Instance, list).ToList();
		int num = Mathf.Min(ContagionStatus.ContagionsCount, list.Count);
		for (int num2 = 0; num2 < num; num2++)
		{
			for (int num3 = 0; num3 < ContagionStatus.Unit.StatusList.Count; num3++)
			{
				TheLastStand.Model.Status.Status status = ContagionStatus.Unit.StatusList[num3];
				if (status.IsFromInjury || !TheLastStand.Model.Status.Status.E_StatusType.AllNegative.HasFlag(status.StatusType))
				{
					continue;
				}
				int num4 = 0;
				UnitStatDefinition.E_Stat stat = UnitStatDefinition.E_Stat.Undefined;
				TheLastStand.Model.Unit.Unit unit = list[num2].Unit;
				if (!(status is StunStatus))
				{
					if (!(status is PoisonStatus poisonStatus))
					{
						if (!(status is ContagionStatus contagionStatus))
						{
							if (status is StatModifierStatus statModifierStatus)
							{
								num4 = (int)statModifierStatus.ModifierValue;
								stat = statModifierStatus.Stat;
							}
						}
						else
						{
							num4 = contagionStatus.ContagionsCount;
						}
					}
					else
					{
						num4 = (int)poisonStatus.DamagePerTurn;
					}
				}
				else
				{
					float randomRange = RandomManager.GetRandomRange(this, 0f, 1f);
					float resistedStunChance = unit.UnitController.GetResistedStunChance(1f);
					if (randomRange >= resistedStunChance)
					{
						continue;
					}
				}
				StatusCreationInfo statusCreationInfo = new StatusCreationInfo
				{
					Source = status.Source,
					Stat = stat,
					TurnsCount = status.RemainingTurnsCount,
					Value = num4
				};
				if (SkillManager.AddStatus(unit, status.StatusType, statusCreationInfo) != null)
				{
					list[num2].Unit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: true);
				}
			}
		}
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			Source = base.Status.Source,
			TurnsCount = base.Status.RemainingTurnsCount,
			Value = ContagionStatus.ContagionsCount,
			IsFromInjury = base.Status.IsFromInjury,
			IsFromPerk = base.Status.IsFromPerk,
			HideDisplayEffect = base.Status.HideDisplayEffect
		};
		return new ContagionStatusController(base.Status.Unit, statusCreationInfo);
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		StyledKeyDisplay pooledComponent = ObjectPooler.GetPooledComponent("StyledKeyDisplay", ResourcePooler.LoadOnce<StyledKeyDisplay>("Prefab/Displayable Effect/UI Effect Displays/StyledKeyDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(base.Status.StatusType);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}

	protected override bool CanBeMerged(TheLastStand.Model.Status.Status otherStatus)
	{
		if (base.Status.GetType() == otherStatus.GetType())
		{
			return base.Status.IsFromInjury == otherStatus.IsFromInjury;
		}
		return false;
	}

	protected override void MergeStatus(TheLastStand.Model.Status.Status otherStatus)
	{
		base.Status.RemainingTurnsCount = Mathf.Max(otherStatus.RemainingTurnsCount, base.Status.RemainingTurnsCount);
	}
}
