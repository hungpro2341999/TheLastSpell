using System.Collections.Generic;
using TheLastStand.Controller.Status;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyAuraAffixController : EnemyAffixController
{
	public EnemyAuraAffix EnemyAuraAffix => base.EnemyAffix as EnemyAuraAffix;

	public EnemyAuraAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyAuraAffix(this, enemyAffixDefinition, enemyUnit);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		if (effectTime == E_EffectTime.OnMovementEnd)
		{
			ApplyAura();
		}
	}

	private void ApplyAura()
	{
		Dictionary<UnitStatDefinition.E_Stat, Node> statModifiers = EnemyAuraAffix.EnemyAuraAffixEffectDefinition.StatModifiers;
		int turnsCount = EnemyAuraAffix.EnemyAuraAffixEffectDefinition.TurnsCount;
		HashSet<EnemyUnit> hashSet = new HashSet<EnemyUnit>();
		foreach (Tile item2 in base.EnemyAffix.EnemyUnit.UnitController.GetTilesInRange(EnemyAuraAffix.Range))
		{
			if (item2.Unit is EnemyUnit item)
			{
				hashSet.Add(item);
			}
		}
		if (!EnemyAuraAffix.EnemyAuraAffixEffectDefinition.IncludeSelf)
		{
			hashSet.Remove(base.EnemyAffix.EnemyUnit);
		}
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, Node> item3 in statModifiers)
		{
			float bonusModifier = EnemyAuraAffix.GetBonusModifier(item3.Key);
			foreach (EnemyUnit item4 in hashSet)
			{
				if (bonusModifier > 0f)
				{
					StatusCreationInfo statusCreationInfo = new StatusCreationInfo
					{
						Source = base.EnemyAffix.EnemyUnit,
						TurnsCount = base.EnemyAffix.EnemyUnit.ComputeStatusDuration(TheLastStand.Model.Status.Status.E_StatusType.Buff, turnsCount),
						Stat = item3.Key,
						Value = bonusModifier
					};
					item4.UnitController.AddStatus(new BuffStatusController(item4, statusCreationInfo).Status);
				}
				else if (bonusModifier < 0f)
				{
					StatusCreationInfo statusCreationInfo2 = new StatusCreationInfo
					{
						Source = base.EnemyAffix.EnemyUnit,
						TurnsCount = base.EnemyAffix.EnemyUnit.ComputeStatusDuration(TheLastStand.Model.Status.Status.E_StatusType.Debuff, turnsCount),
						Stat = item3.Key,
						Value = 0f - bonusModifier
					};
					item4.UnitController.AddStatus(new DebuffStatusController(item4, statusCreationInfo2).Status);
				}
			}
		}
	}
}
