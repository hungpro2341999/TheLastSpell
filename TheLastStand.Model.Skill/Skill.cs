using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Skill;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Skill;

public class Skill : ISerializable, IDeserializable
{
	public enum E_ComputationStat
	{
		OverallDamage,
		Critical,
		CriticalPower,
		Reliability,
		Accuracy,
		OpportunisticAttacks,
		IsolatedAttacks,
		MomentumAttacks,
		PoisonDamageModifier,
		BuffDurationModifier,
		ContagionDurationModifier,
		DebuffDurationModifier,
		PoisonDurationModifier,
		StunDurationModifier,
		ActionPointsCost,
		ManaCost,
		HealthCost,
		MovePointsCost,
		FlatResistanceReduction,
		PercentageResistanceReduction,
		MeleeArmorShreddingBonus,
		FlatDamage
	}

	public class SkillBarIndexComparer : IComparer<Skill>
	{
		public int Compare(Skill a, Skill b)
		{
			return a.GetSkillBarOrder().CompareTo(b.GetSkillBarOrder());
		}
	}

	public static class Constants
	{
		public static class SkillId
		{
			public const string JumpOverWall = "JumpOverWall";

			public const string Leapfrog = "JumpOverWall2";

			public const string MartialPunch = "MartialPunch";

			public const string FinishHim = "FinishHim";

			public const string Punch = "Punch";

			public const string EmergencyTunnel = "EmergencyTunnelSkill";

			public const string ElvenCoffee = "ElvenCoffee";
		}
	}

	private int usesPerTurnRemaining;

	public const string SerializationElementName = "Skill";

	public static readonly SkillBarIndexComparer SharedSkillBarIndexComparer = new SkillBarIndexComparer();

	public string Description
	{
		get
		{
			if (!Localizer.dictionary.ContainsKey("SkillDescription_" + SkillDefinition.LocalizationId))
			{
				return string.Empty;
			}
			return Localizer.Get("SkillDescription_" + SkillDefinition.LocalizationId);
		}
	}

	public string Name => Localizer.Get("SkillName_" + SkillDefinition.LocalizationId);

	public bool IsLinkedWithAnotherSkillForUses => LinkedSkillForUses != null;

	public Skill LinkedSkillForUses { get; set; }

	public int OverallUsesRemaining { get; set; }

	public int OverallUses { get; set; }

	public TheLastStand.Model.Skill.SkillAction.SkillAction SkillAction { get; set; }

	public ISkillContainer SkillContainer { get; private set; }

	public SkillController SkillController { get; private set; }

	public SkillDefinition SkillDefinition { get; private set; }

	public ISkillCaster OverridenOwner { get; set; }

	public ISkillCaster Owner => SkillContainer?.Holder;

	public ISkillCaster OwnerOrSelected => Owner ?? TileObjectSelectionManager.SelectedPlayableUnit;

	public TileObjectSelectionManager.E_Orientation CursorDependantOrientation
	{
		get
		{
			if (!SkillDefinition.LockAutoOrientation)
			{
				return TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection;
			}
			return TileObjectSelectionManager.E_Orientation.NONE;
		}
	}

	public int PerkLocksBuffer
	{
		get
		{
			if (SkillContainer is PlayableUnit playableUnit)
			{
				return playableUnit.SkillLocksBuffers.GetValueOrDefault(SkillDefinition.Id);
			}
			if (SkillContainer is Perk { Owner: not null } perk)
			{
				return perk.Owner.SkillLocksBuffers.GetValueOrDefault(SkillDefinition.Id);
			}
			return 0;
		}
	}

	public List<ITileObject> Targets { get; set; }

	public int UsesPerTurnCountFromDefinition
	{
		get
		{
			if (IsLinkedWithAnotherSkillForUses)
			{
				return LinkedSkillForUses.SkillDefinition.UsesPerTurnCount;
			}
			return SkillDefinition.UsesPerTurnCount;
		}
	}

	public int UsesPerTurnRemaining
	{
		get
		{
			if (IsLinkedWithAnotherSkillForUses)
			{
				return LinkedSkillForUses.UsesPerTurnRemaining;
			}
			return usesPerTurnRemaining;
		}
	}

	public bool HasBaseManaOrHealthCost
	{
		get
		{
			if (BaseManaCost <= 0)
			{
				return BaseHealthCost > 0;
			}
			return true;
		}
	}

	public bool ProvidedByItem => SkillContainer is TheLastStand.Model.Item.Item;

	public bool IsJump
	{
		get
		{
			if (!(SkillDefinition.Id == "JumpOverWall"))
			{
				return SkillDefinition.Id == "JumpOverWall2";
			}
			return true;
		}
	}

	public bool IsPunch
	{
		get
		{
			if (!(SkillDefinition.Id == "Punch"))
			{
				return SkillDefinition.Id == "MartialPunch";
			}
			return true;
		}
	}

	public bool IsSingleTarget => SkillDefinition.AreaOfEffectDefinition.IsSingleTarget;

	public bool IsMagicalDamage
	{
		get
		{
			if (SkillAction is AttackSkillAction attackSkillAction)
			{
				return attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Magical;
			}
			return false;
		}
	}

	public bool IsPhysicalDamage
	{
		get
		{
			if (SkillAction is AttackSkillAction attackSkillAction)
			{
				return attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Physical;
			}
			return false;
		}
	}

	public bool IsRangedDamage
	{
		get
		{
			if (SkillAction is AttackSkillAction attackSkillAction)
			{
				return attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Ranged;
			}
			return false;
		}
	}

	public bool DamageTypeDiffersFromOwnerLastSkill
	{
		get
		{
			if (SkillAction is AttackSkillAction attackSkillAction && Owner is TheLastStand.Model.Unit.Unit { LastSkillType: not AttackSkillActionDefinition.E_AttackType.None } unit)
			{
				return attackSkillAction.AttackType != unit.LastSkillType;
			}
			return false;
		}
	}

	public int ManaCost => SkillAction.SkillActionController.ComputeManaCost();

	public int ActionPointsCost => SkillAction.SkillActionController.ComputeActionPointsCost();

	public int HealthCost => SkillAction.SkillActionController.ComputeHealthCost();

	public int MovePointsCost => SkillAction.SkillActionController.ComputeMovePointsCost();

	public int UsesPerTurn => SkillAction.SkillActionController.ComputeUsesPerTurn();

	public int BaseManaCost => SkillDefinition.ManaCost;

	public int BaseActionPointsCost => SkillDefinition.ActionPointsCost;

	public int BaseHealthCost => SkillAction.SkillActionController.ComputeBaseHealthCost();

	public int BaseMovePointsCost => SkillDefinition.MovePointsCost;

	public string Id => SkillDefinition.Id;

	public bool IsAttack => SkillAction is AttackSkillAction;

	public bool IsAttackOrExecute
	{
		get
		{
			if (!IsAttack)
			{
				return HasExecute;
			}
			return true;
		}
	}

	public bool IsAttackOrExecuteOrSurroundingDamage
	{
		get
		{
			if (!IsAttackOrExecute)
			{
				return HasSurroundingDamage;
			}
			return true;
		}
	}

	public int MinRange => SkillDefinition.Range.x;

	public float BaseMaxRange => SkillDefinition.Range.y;

	public float MaxRange => (SkillContainer.Holder is TheLastStand.Model.Unit.Unit unit) ? unit.UnitController.GetModifiedMaxRange(this) : SkillDefinition.Range.y;

	public bool HasManeuver => SkillAction.HasEffect("Maneuver");

	public bool HasFollow => SkillAction.HasEffect("Follow");

	public bool HasMomentum => SkillAction.HasEffect("Momentum");

	public bool HasNoMomentum => SkillAction.HasEffect("NoMomentum");

	public bool HasBuff
	{
		get
		{
			if (!SkillAction.HasEffect("Buff"))
			{
				return SkillAction.HasSurroundingEffect<BuffEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasContagion
	{
		get
		{
			if (!SkillAction.HasEffect("Contagion"))
			{
				return SkillAction.HasSurroundingEffect<ContagionEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasDebuff
	{
		get
		{
			if (!SkillAction.HasEffect("Debuff"))
			{
				return SkillAction.HasSurroundingEffect<DebuffEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasExecute
	{
		get
		{
			if (!SkillAction.HasEffect("Kill"))
			{
				return SkillAction.HasSurroundingEffect<KillSkillEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasExtinguish
	{
		get
		{
			if (!SkillAction.HasEffect("ExtinguishBrazier"))
			{
				return SkillAction.HasSurroundingEffect<ExtinguishBrazierSkillEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasPoison
	{
		get
		{
			if (!SkillAction.HasEffect("Poison"))
			{
				return SkillAction.HasSurroundingEffect<PoisonEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasStun
	{
		get
		{
			if (!SkillAction.HasEffect("Stun"))
			{
				return SkillAction.HasSurroundingEffect<StunEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasRemoveStatus
	{
		get
		{
			if (!SkillAction.HasEffect<RemoveStatusEffectDefinition>())
			{
				return SkillAction.HasCasterEffect<RemoveStatusEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasSurroundingDamage => SkillAction.HasSurroundingEffect<DamageSurroundingEffectDefinition>();

	public bool HasVision
	{
		get
		{
			if (!SkillAction.HasEffect("IgnoreLineOfSight"))
			{
				return SkillAction.HasSurroundingEffect<IgnoreLineOfSightEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNegativeAlteration
	{
		get
		{
			if (!HasDebuff && !HasPoison && !HasStun)
			{
				return HasContagion;
			}
			return true;
		}
	}

	public bool HasPropagation => SkillAction.HasEffect("Propagation");

	public bool HasNativeManeuver => SkillAction.HasNativeEffect("Maneuver");

	public bool HasNativeFollow => SkillAction.HasNativeEffect("Follow");

	public bool HasNativeMomentum => SkillAction.HasNativeEffect("Momentum");

	public bool HasNativeNoMomentum => SkillAction.HasNativeEffect("NoMomentum");

	public bool HasNativeBuff
	{
		get
		{
			if (!SkillAction.HasNativeEffect("Buff"))
			{
				return SkillAction.HasNativeSurroundingEffect<BuffEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativeContagion
	{
		get
		{
			if (!SkillAction.HasNativeEffect("Contagion"))
			{
				return SkillAction.HasNativeSurroundingEffect<ContagionEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativeDebuff
	{
		get
		{
			if (!SkillAction.HasNativeEffect("Debuff"))
			{
				return SkillAction.HasNativeSurroundingEffect<DebuffEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativeExecute
	{
		get
		{
			if (!SkillAction.HasNativeEffect("Kill"))
			{
				return SkillAction.HasNativeSurroundingEffect<KillSkillEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativeExtinguish
	{
		get
		{
			if (!SkillAction.HasNativeEffect("ExtinguishBrazier"))
			{
				return SkillAction.HasNativeSurroundingEffect<ExtinguishBrazierSkillEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativePoison
	{
		get
		{
			if (!SkillAction.HasNativeEffect("Poison"))
			{
				return SkillAction.HasNativeSurroundingEffect<PoisonEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativeStun
	{
		get
		{
			if (!SkillAction.HasNativeEffect("Stun"))
			{
				return SkillAction.HasNativeSurroundingEffect<StunEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativeRemoveStatus
	{
		get
		{
			if (!SkillAction.HasNativeEffect<RemoveStatusEffectDefinition>())
			{
				return SkillAction.HasNativeCasterEffect<RemoveStatusEffectDefinition>();
			}
			return true;
		}
	}

	public bool HasNativeSurroundingDamage => SkillAction.HasNativeSurroundingEffect<DamageSurroundingEffectDefinition>();

	public bool HasNativeVision => SkillAction.HasNativeEffect("IgnoreLineOfSight");

	public bool HasNativeAlteration
	{
		get
		{
			if (!HasNativeDebuff && !HasNativePoison && !HasNativeStun)
			{
				return HasNativeContagion;
			}
			return true;
		}
	}

	public bool HasNativePropagation => SkillAction.HasNativeEffect("Propagation");

	public Skill(SerializedSkill container, SkillController skillController, ISkillContainer skillContainer)
	{
		SkillController = skillController;
		SkillContainer = skillContainer;
		Deserialize(container);
	}

	public Skill(SkillDefinition skillDefinition, SkillController skillController, ISkillContainer container, int overallUsesCount = -1, int usesPerTurnCount = -1, int bonusUses = 0)
	{
		SkillDefinition = skillDefinition;
		SkillController = skillController;
		SkillContainer = container;
		OverallUsesRemaining = overallUsesCount;
		OverallUses = overallUsesCount;
		SetUsesPerTurnRemaining(usesPerTurnCount);
	}

	public TileObjectSelectionManager.E_Orientation TileDependantOrientation(Tile tile)
	{
		if (!SkillDefinition.LockAutoOrientation)
		{
			return TileObjectSelectionManager.GetOrientationFromTileToTile(SkillAction.SkillActionExecution.SkillSourceTiles.GetFirstClosestTile(tile).tile, tile);
		}
		return TileObjectSelectionManager.E_Orientation.NONE;
	}

	public int ComputeTotalUses(ISkillCaster owner = null)
	{
		owner = owner ?? Owner;
		if (owner == null || owner is BattleModule)
		{
			return OverallUses;
		}
		if (owner is PlayableUnit playableUnit && SkillContainer is TheLastStand.Model.Item.Item item && ItemDefinition.E_Category.Usable.HasFlag(item.ItemDefinition.Category))
		{
			if (item.SkillsOverallUses.ContainsKey(SkillDefinition.Id))
			{
				return item.SkillsOverallUses[SkillDefinition.Id] + (int)playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.BonusUsableItemsUses).FinalClamped;
			}
			return OverallUses + (int)playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.BonusUsableItemsUses).FinalClamped;
		}
		return OverallUses;
	}

	public void SetUsesPerTurnRemaining(int newValue, bool applyChangeToLinkedSkills = false)
	{
		if (!IsLinkedWithAnotherSkillForUses)
		{
			usesPerTurnRemaining = newValue;
		}
		else if (applyChangeToLinkedSkills)
		{
			LinkedSkillForUses.SetUsesPerTurnRemaining(newValue);
		}
	}

	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedSkill serializedSkill = container as SerializedSkill;
		SkillDefinition = SkillDatabase.SkillDefinitions[serializedSkill.Id];
		OverallUsesRemaining = serializedSkill.OverallUsesRemaining;
		OverallUses = serializedSkill.OverallUsesRemainingBase;
		SetUsesPerTurnRemaining(serializedSkill.UsesPerTurnRemaining);
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedSkill
		{
			Id = SkillDefinition.Id,
			OverallUsesRemaining = OverallUsesRemaining,
			OverallUsesRemainingBase = OverallUses,
			UsesPerTurnRemaining = UsesPerTurnRemaining
		};
	}

	private int GetSkillBarOrder()
	{
		if (IsJump)
		{
			return -3;
		}
		if (SkillDefinition.IsBrazierSpecific)
		{
			return -2;
		}
		if (SkillDefinition.IsLockedByPerk)
		{
			return -1;
		}
		return 0;
	}

	public bool HasRemoveStatusWithType(string statusString)
	{
		if (!Enum.TryParse<TheLastStand.Model.Status.Status.E_StatusType>(statusString.Trim(), out var result))
		{
			TPSingleton<SkillManager>.Instance.LogError($"Checking if the skill {SkillDefinition.Id} HasRemoveStatusWithType {result} but this status type does not exist.");
			return false;
		}
		return HasRemoveStatusWithTypeInternal(result);
	}

	public bool IsFromItemWithCategory(string category)
	{
		if (SkillContainer is TheLastStand.Model.Item.Item item)
		{
			string[] array = category.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (Enum.TryParse<ItemDefinition.E_Category>(array[i].Trim(), out var result) && item.ItemDefinition.Category.HasFlag(result))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsFromItemWithTag(string tag)
	{
		ISkillContainer skillContainer = SkillContainer;
		TheLastStand.Model.Item.Item item = skillContainer as TheLastStand.Model.Item.Item;
		if (item != null)
		{
			return tag.Split(',').Any((string aTag) => item.ItemDefinition.HasTag(aTag.Trim()));
		}
		return false;
	}

	public bool IsSkillID(string SkillId)
	{
		return Id == SkillId;
	}

	private bool HasRemoveStatusWithTypeInternal(TheLastStand.Model.Status.Status.E_StatusType statusType)
	{
		List<RemoveStatusEffectDefinition> list = new List<RemoveStatusEffectDefinition>();
		if (SkillAction.TryGetEffects("RemoveStatus", out List<RemoveStatusEffectDefinition> effects, onlyNative: false))
		{
			list.AddRange(effects);
		}
		if (SkillAction.TryGetEffects("Dispel", out List<RemoveStatusEffectDefinition> effects2, onlyNative: false))
		{
			list.AddRange(effects2);
		}
		if (SkillAction.TryGetCasterEffects(out List<RemoveStatusEffectDefinition> effects3))
		{
			list.AddRange(effects3);
		}
		foreach (RemoveStatusEffectDefinition item in list)
		{
			if (item.RemoveStatusDefinition.Status == statusType || item.RemoveStatusDefinition.Status.HasFlag(statusType))
			{
				return true;
			}
		}
		return false;
	}
}
