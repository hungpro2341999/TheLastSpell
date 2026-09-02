using System;
using System.Collections.Generic;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Model.Skill.SkillAction;

public class AttackSkillAction : SkillAction
{
	public static readonly AttackSkillActionDefinition.E_AttackType[] attackTypesByPriority = new AttackSkillActionDefinition.E_AttackType[3]
	{
		AttackSkillActionDefinition.E_AttackType.Physical,
		AttackSkillActionDefinition.E_AttackType.Magical,
		AttackSkillActionDefinition.E_AttackType.Ranged
	};

	public AttackSkillActionController AttackSkillActionController => base.SkillActionController as AttackSkillActionController;

	public AttackSkillActionDefinition AttackSkillActionDefinition => base.SkillActionDefinition as AttackSkillActionDefinition;

	public AttackSkillActionExecution AttackSkillActionExecution => base.SkillActionExecution as AttackSkillActionExecution;

	public AttackSkillActionDefinition.E_AttackType AttackType
	{
		get
		{
			if (AttackSkillActionDefinition.AttackType == AttackSkillActionDefinition.E_AttackType.Adaptative)
			{
				PlayableUnit playableUnit = (base.Skill.SkillContainer?.Holder as PlayableUnit) ?? TileObjectSelectionManager.SelectedPlayableUnit;
				if (playableUnit != null)
				{
					List<(float, AttackSkillActionDefinition.E_AttackType)> list = new List<(float, AttackSkillActionDefinition.E_AttackType)>(3);
					list.Add((playableUnit.PhysicalDamage, AttackSkillActionDefinition.E_AttackType.Physical));
					list.Add((playableUnit.MagicalDamage, AttackSkillActionDefinition.E_AttackType.Magical));
					list.Add((playableUnit.RangedDamage, AttackSkillActionDefinition.E_AttackType.Ranged));
					list.Sort(((float, AttackSkillActionDefinition.E_AttackType) a, (float, AttackSkillActionDefinition.E_AttackType) b) => (a.Item1 != b.Item1) ? b.Item1.CompareTo(a.Item1) : Array.IndexOf(attackTypesByPriority, a.Item2).CompareTo(Array.IndexOf(attackTypesByPriority, b.Item2)));
					return list[0].Item2;
				}
			}
			return AttackSkillActionDefinition.AttackType;
		}
	}

	public override string EstimationIconId => "Attack";

	public List<TheLastStand.Model.Building.Building> HeroHitsBuildings { get; set; } = new List<TheLastStand.Model.Building.Building>();

	public int HeroHitsEnemyCount { get; set; }

	public int HeroHitsHeroCount { get; set; }

	public Vector2 BaseDamage
	{
		get
		{
			TheLastStand.Model.Unit.Unit formulaInterpreterContext = base.Skill.Owner as TheLastStand.Model.Unit.Unit;
			Vector2 baseDamage = AttackSkillActionDefinition.GetBaseDamage(formulaInterpreterContext);
			if (baseDamage != Vector2.zero)
			{
				return baseDamage;
			}
			if (ItemContainer != null)
			{
				return ItemContainer.BaseDamages;
			}
			return Vector2.zero;
		}
	}

	public TheLastStand.Model.Item.Item ItemContainer => base.Skill.SkillContainer as TheLastStand.Model.Item.Item;

	public AttackSkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}
}
