using System;
using System.Collections.Generic;
using System.Linq;
using PortraitAPI;
using PortraitAPI.Misc;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Unit;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Item;
using TheLastStand.Serialization.Perk;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit;

public class PlayableUnit : Unit, ISkillContainer, IPerkUnlocker
{
	public static class Constants
	{
		public static class Gender
		{
			public const string Male = "Male";

			public const string Female = "Female";
		}

		public static class Datas
		{
			public const int MinNameSize = 1;

			public const int MaxNameSize = 20;
		}

		public static class Perks
		{
			public const string BackProtectionBuildings = "BackProtectionBuildings";
		}
	}

	public class StringToTraitIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.UnitTraitDefinitions.Keys);
	}

	public class StringToCurrentTraitsConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				for (int num = TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions.Count - 1; num >= 0; num--)
				{
					list.Add(TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions[num].Id);
				}
				return list;
			}
		}
	}

	public class StringToHairPaletteIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.PlayableUnitHairColorDefinitions.Keys);
	}

	public class StringToPortraitIdConverter : StringToStringCollectionEntryConverter
	{
		public static class Constants
		{
			public const string RandomValue = "Random";
		}

		protected override List<string> Entries => PlayableUnitView.FaceIdAvailablePortraitIds[TileObjectSelectionManager.SelectedPlayableUnit.FaceId];

		public override bool TryConvert(string value, out object result)
		{
			if (!base.TryConvert(value, out result))
			{
				if (string.Equals(value, "Random", StringComparison.OrdinalIgnoreCase))
				{
					result = 0;
					return true;
				}
				return false;
			}
			return true;
		}

		public override List<string> GetAutoCompleteTexts(string argument)
		{
			List<string> autoCompleteTexts = base.GetAutoCompleteTexts(argument);
			autoCompleteTexts.Insert(0, "Random");
			return autoCompleteTexts;
		}
	}

	public class StringToRaceIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.RaceDefinitions.Keys);
	}

	public class StringToSkinPaletteIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.Keys);
	}

	public class StringToStatIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				foreach (KeyValuePair<UnitStatDefinition.E_Stat, UnitStatDefinition> unitStatDefinition in UnitDatabase.UnitStatDefinitions)
				{
					list.Add(unitStatDefinition.Key.ToString());
				}
				return list;
			}
		}
	}

	public class StringToFaceIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.GetFaceIdsForGender(TileObjectSelectionManager.SelectedPlayableUnit.Gender));
	}

	public int MomentumTilesActive;

	public int TotalMomentumTilesCrossedThisTurn;

	public int TilesCrossedThisTurn;

	public int ActionPointsSpentThisTurn;

	public float AdditionalNightExperience { get; set; }

	public string AnalyticsIdentifier => $"{ArchetypeId}_{base.RandomId}";

	public string ArchetypeId { get; set; }

	public override RaceDefinition BarkerRaceDefinition => RaceDefinition;

	public Dictionary<string, BodyPart> BodyParts { get; } = new Dictionary<string, BodyPart>();

	public List<TheLastStand.Model.Skill.Skill> ContextualSkills { get; set; }

	public List<TheLastStand.Model.Skill.Skill> NativeSkills { get; } = new List<TheLastStand.Model.Skill.Skill>();

	public List<TheLastStand.Model.Skill.Skill> MomentumSkills => PlayableUnitController.GetAllSkillsNoCheck().FindAll((TheLastStand.Model.Skill.Skill s) => s.HasMomentum);

	public Dictionary<string, int> SkillLocksBuffers { get; } = new Dictionary<string, int>();

	public Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> EquipmentSlots { get; } = new Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>>();

	public int EquippedWeaponSetIndex { get; set; }

	public float Experience { get; set; }

	public float ExperienceInCurrentLevel { get; set; }

	public float ExperienceNeededToNextLevel { get; set; }

	public string FaceId { get; set; }

	public string Gender { get; set; }

	public ColorSwapPaletteDefinition HairColorPalette
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PlayableUnitHairColorDefinitions.ElementAt(PortraitCodeData.CodeColorDatas[Commons.E_ColorTypes.Hair].Index).Value;
		}
	}

	public bool HelmetDisplayed { get; set; } = true;

	public ISkillCaster Holder => this;

	public override Vector2 HUDOffset => RaceDefinition?.HUDOffset ?? Vector2.zero;

	public ColorSwapPaletteDefinition EyesColorPalette
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.ElementAt(PortraitCodeData.CodeColorDatas[Commons.E_ColorTypes.Eyes].Index).Value;
		}
	}

	public override string Id => PlayableUnitName;

	public bool IsStartingUnit { get; set; }

	public override string Name => PlayableUnitName;

	public float LastTurnHealth { get; set; }

	public double Level { get; set; } = 1.0;

	public int LevelPoints => UnitLevelUpPoints.Count;

	public UnitLevelUp LevelUp { get; set; }

	public LifetimeStats LifetimeStats { get; set; }

	public int MainStatsPoints
	{
		get
		{
			int num = 0;
			for (int i = 0; i < UnitLevelUpPoints.Count; i++)
			{
				if (UnitLevelUpPoints[i].HasMainStatPoint)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool MovedThisDay { get; set; }

	public override bool OverrideDefaultHUDOffset => RaceDefinition?.OverrideDefaultHUDOffset ?? false;

	public List<AddSkillEffect> PerkAddedSkillEffects { get; } = new List<AddSkillEffect>();

	public Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<ComputationStatLockerEffect>> PerkComputationStatsLocksBuffer { get; } = new Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<ComputationStatLockerEffect>>();

	public Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<SkillModifierEffect>> PerkSkillModifierEffects { get; } = new Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<SkillModifierEffect>>();

	public List<AllowDiagonalPropagationEffect> AllowDiagonalPropagationEffects { get; } = new List<AllowDiagonalPropagationEffect>();

	public int PerksPoints { get; set; }

	public int PerkRerollCount { get; set; }

	public UnitPerkTree PerkTree { get; set; }

	public PlayableUnitController PlayableUnitController => base.UnitController as PlayableUnitController;

	public PlayableUnitPerksController PlayableUnitPerksController { get; private set; }

	public PlayableUnitStatsController PlayableUnitStatsController => base.UnitStatsController as PlayableUnitStatsController;

	public string PlayableUnitName { get; set; }

	public PlayableUnitView PlayableUnitView { get; private set; }

	public DataColor PortraitColor
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PortraitBackgroundColors[PortraitCodeData.CodeColorDatas[Commons.E_ColorTypes.Background].Index];
		}
	}

	public Sprite PortraitSprite { get; set; }

	public Sprite PortraitBackgroundSprite { get; set; }

	public CodeGenerator.CodeData PortraitCodeData { get; set; }

	public RaceDefinition RaceDefinition { get; set; }

	public int SecondaryStatsPoints
	{
		get
		{
			int num = 0;
			for (int i = 0; i < UnitLevelUpPoints.Count; i++)
			{
				if (UnitLevelUpPoints[i].HasSecondaryStatPoint)
				{
					num++;
				}
			}
			return num;
		}
	}

	public ColorSwapPaletteDefinition SkinColorPalette
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.ElementAt(PortraitCodeData.CodeColorDatas[Commons.E_ColorTypes.Skin].Index).Value;
		}
	}

	public int StatsPoints => MainStatsPoints + SecondaryStatsPoints;

	public override UnitView UnitView
	{
		get
		{
			return base.UnitView;
		}
		set
		{
			base.UnitView = value;
			PlayableUnitView = UnitView as PlayableUnitView;
		}
	}

	public List<UnitLevelUpPoint> UnitLevelUpPoints { get; private set; } = new List<UnitLevelUpPoint>();

	public Dictionary<string, TheLastStand.Model.Unit.Perk.Perk> Perks { get; } = new Dictionary<string, TheLastStand.Model.Unit.Perk.Perk>();

	public int UnlockedPerksCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, TheLastStand.Model.Unit.Perk.Perk> perk in Perks)
			{
				if (perk.Value.UnlockedInPerkTree)
				{
					num++;
				}
			}
			return num;
		}
	}

	public List<UnitTraitDefinition> UnitTraitDefinitions { get; } = new List<UnitTraitDefinition>();

	private int HeroesCount => TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count;

	public float UnlockedArmorTotal => PlayableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ArmorTotal).FinalUnlockedClamped;

	public float UnlockedManaTotal => PlayableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaTotal).FinalUnlockedClamped;

	public float UnlockedManaRegen => PlayableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaRegen).FinalUnlockedClamped;

	public float UnlockedPropagationBouncesModifier => PlayableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.PropagationBouncesModifier).FinalUnlockedClamped;

	public float ActionPoints => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).FinalClamped;

	public float HealthRegen => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthRegen).FinalClamped;

	public float MagicalDamage => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MagicalDamage).FinalClamped;

	public float PhysicalDamage => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PhysicalDamage).FinalClamped;

	public float RangedDamage => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.RangedDamage).FinalClamped;

	public float OverallDamage => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.OverallDamage).FinalClamped;

	public float ResistanceReduction => GetClampedStatValue(UnitStatDefinition.E_Stat.ResistanceReduction);

	public float PercentageResistanceReduction => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).FinalClamped;

	public float SkillRangeModifier => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.SkillRangeModifier).FinalClamped;

	public float PoisonDamageModifier => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PoisonDamageModifier).FinalClamped;

	public int TrinketsLevels => GetEquippedTrinketsLevels();

	public int ClosestAllyDistance
	{
		get
		{
			int num = int.MaxValue;
			foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				if (playableUnit != this)
				{
					int num2 = TileMapController.DistanceBetweenTiles(base.OriginTile, playableUnit.OriginTile);
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
			return num;
		}
	}

	public bool IsBackProtectionValid
	{
		get
		{
			IdsListDefinition backProtectionIds = GenericDatabase.IdsListDefinitions["BackProtectionBuildings"];
			return PlayableUnitController.GetAdjacentTiles().Any((Tile tile) => tile.Building != null && backProtectionIds.Ids.Contains(tile.Building.Id));
		}
	}

	public int OffHandEquippedItemsNumber
	{
		get
		{
			if (!EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value))
			{
				return 0;
			}
			return value.Count((EquipmentSlot equipmentSlot) => equipmentSlot.Item != null);
		}
	}

	public int OneHandEquippedItemsNumber
	{
		get
		{
			if (!EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out var value))
			{
				return 0;
			}
			return value.Count((EquipmentSlot equipmentSlot) => equipmentSlot.Item != null && !equipmentSlot.Item.IsTwoHandedWeapon);
		}
	}

	public int TwoHandEquippedItemsNumber
	{
		get
		{
			if (!EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out var value))
			{
				return 0;
			}
			return value.Count((EquipmentSlot equipmentSlot) => equipmentSlot.Item != null && equipmentSlot.Item.IsTwoHandedWeapon);
		}
	}

	public bool IsActionPointsCostLocked => IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.ActionPointsCost);

	public bool IsManaCostLocked => IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.ManaCost);

	public bool IsHealthCostLocked => IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.HealthCost);

	public PlayableUnit(UnitTemplateDefinition unitTemplateDefinition, SerializedPlayableUnit serializedPlayableUnit, UnitController unitController, int saveVersion, bool isDead)
		: base(unitTemplateDefinition, unitController)
	{
		Deserialize(serializedPlayableUnit, saveVersion, isDead);
		Init();
		InitLifetimeStats(serializedPlayableUnit.LifetimeStats);
	}

	public PlayableUnit(UnitTemplateDefinition unitTemplateDefinition, UnitController unitController, UnitView unitView, string archetypeId, bool isStartingUnit = false)
		: base(unitTemplateDefinition, unitController, unitView)
	{
		ArchetypeId = archetypeId;
		base.RandomId = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, int.MaxValue);
		PlayableUnitPerksController = new PlayableUnitPerksController(this);
		IsStartingUnit = isStartingUnit;
		Init();
		InitLifetimeStats();
	}

	public bool AllowDiagonalPropagation(PerkDataContainer perkDataContainer)
	{
		return AllowDiagonalPropagationEffects.Any((AllowDiagonalPropagationEffect x) => x.PerkDataConditions.IsValid(perkDataContainer));
	}

	public override int ComputeStatusDuration(TheLastStand.Model.Status.Status.E_StatusType statusType, int baseValue, PerkDataContainer perkDataContainer = null, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		if (baseValue == -1)
		{
			return -1;
		}
		int num = 0;
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Poison) != TheLastStand.Model.Status.Status.E_StatusType.None)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.PoisonDurationModifier, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PoisonDurationModifier));
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.PoisonDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Debuff) != TheLastStand.Model.Status.Status.E_StatusType.None)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.DebuffDurationModifier, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.DebuffDurationModifier));
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.DebuffDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Buff) != TheLastStand.Model.Status.Status.E_StatusType.None)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.BuffDurationModifier, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.BuffDurationModifier));
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.BuffDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Stun) != TheLastStand.Model.Status.Status.E_StatusType.None)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.StunDurationModifier, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.StunDurationModifier));
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.StunDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Contagion) != TheLastStand.Model.Status.Status.E_StatusType.None)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.ContagionDurationModifier, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.ContagionDurationModifier));
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.ContagionDurationModifier, perkDataContainer);
		}
		return Mathf.Max(1, baseValue + num);
	}

	public float GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat, PerkDataContainer perkDataContainer, bool? affectBase = null)
	{
		if (perkDataContainer == null || !PerkSkillModifierEffects.ContainsKey(computationStat) || PerkSkillModifierEffects[computationStat] == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (SkillModifierEffect item in PerkSkillModifierEffects[computationStat])
		{
			bool flag = !affectBase.HasValue || affectBase == item.SkillModifierEffectDefinition.AffectBase;
			if (!item.HasBeenUsed && flag && item.PerkDataConditions.IsValid(perkDataContainer))
			{
				item.HasBeenUsed = true;
				num += item.Value;
				item.HasBeenUsed = false;
			}
		}
		return num;
	}

	public bool IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat)
	{
		if (PerkComputationStatsLocksBuffer.TryGetValue(computationStat, out var value))
		{
			return value.Any((ComputationStatLockerEffect effect) => effect.PerkDataConditions.IsValid(null, updatePerkTargetObject: false));
		}
		return false;
	}

	public void RegisterBodyPartViews(BodyPartView[] bodyPartViews, bool register)
	{
		if (PlayableUnitView == null)
		{
			return;
		}
		BodyPart value = null;
		int i = 0;
		for (int num = bodyPartViews.Length; i < num; i++)
		{
			if (!(bodyPartViews[i] != null))
			{
				continue;
			}
			if (!BodyParts.TryGetValue(bodyPartViews[i].name, out value))
			{
				TPSingleton<PlayableUnitManager>.Instance.LogError($"Trying to register an invalid BodyPartView to a BodyPart (Bodypart view : {bodyPartViews[i]}", CLogLevel.DETAILED);
				continue;
			}
			if (register)
			{
				bodyPartViews[i].BodyPart = value;
				value.SetBodyPartView(bodyPartViews[i].Orientation, bodyPartViews[i]);
				bodyPartViews[i].IsDirty = true;
				continue;
			}
			if (bodyPartViews[i].BodyPart == value)
			{
				bodyPartViews[i].BodyPart = null;
				bodyPartViews[i].IsDirty = true;
			}
			_ = value.GetBodyPartView(bodyPartViews[i].Orientation) == bodyPartViews[i];
		}
	}

	public void ToggleContextualSkillLock(string skillId, bool locks, TheLastStand.Model.Unit.Perk.Perk perkContainer = null, int overallUses = -1)
	{
		if (locks)
		{
			TheLastStand.Model.Skill.Skill skill = ContextualSkills.Find((TheLastStand.Model.Skill.Skill x) => x.SkillDefinition.Id == skillId);
			if (skill != null)
			{
				ContextualSkills.Remove(skill);
			}
		}
		else if (ContextualSkills.All((TheLastStand.Model.Skill.Skill x) => x.SkillDefinition.Id != skillId))
		{
			if (!SkillDatabase.SkillDefinitions.TryGetValue(skillId, out var value))
			{
				TPSingleton<PlayableUnitManager>.Instance.LogError("Skill " + skillId + " not found!");
				return;
			}
			TheLastStand.Model.Skill.Skill skill2 = new SkillController(value, (ISkillContainer)(((object)perkContainer) ?? ((object)this)), overallUses, value.UsesPerTurnCount).Skill;
			ContextualSkills.Add(skill2);
		}
	}

	public override string ToString()
	{
		string name = Name;
		name += "\n";
		foreach (UnitStatDefinition.E_Stat statsKey in base.UnitStatsController.UnitStats.StatsKeys)
		{
			name += $"{statsKey}: {base.UnitStatsController.GetStat(statsKey).FinalClamped}\n";
		}
		name += "\n";
		for (int i = 0; i < UnitTraitDefinitions.Count; i++)
		{
			name = name + "Trait " + UnitTraitDefinitions[i].Id + "\n";
		}
		name += "\n";
		foreach (KeyValuePair<string, TheLastStand.Model.Unit.Perk.Perk> perk in Perks)
		{
			if (perk.Value.Unlocked)
			{
				name = name + "Perk " + perk.Value.PerkDefinition.Id + "\n";
			}
		}
		return name;
	}

	protected override bool ComputeIsolation()
	{
		List<Tile> adjacentTiles = base.UnitController.GetAdjacentTiles();
		for (int num = adjacentTiles.Count - 1; num >= 0; num--)
		{
			if (adjacentTiles[num]?.Unit != null && adjacentTiles[num].Unit != this && adjacentTiles[num].Unit is PlayableUnit)
			{
				return false;
			}
		}
		return true;
	}

	protected override void Init()
	{
		base.Init();
		foreach (KeyValuePair<string, BodyPartDefinition> playableUnitNakedBodyPartsDefinition in PlayableUnitDatabase.PlayableUnitNakedBodyPartsDefinitions)
		{
			BodyParts.Add(playableUnitNakedBodyPartsDefinition.Key, new BodyPart(playableUnitNakedBodyPartsDefinition.Value));
		}
	}

	private int GetEquippedTrinketsLevels()
	{
		int num = 0;
		if (EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.Trinket, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].Item != null && value[i].Item.ItemDefinition.Category == ItemDefinition.E_Category.Trinket)
				{
					num += value[i].Item.Level;
				}
			}
		}
		return num;
	}

	private void InitLifetimeStats(SerializedLifetimeStats container = null)
	{
		if (container != null)
		{
			LifetimeStats = new LifetimeStatsController(container).LifetimeStats;
		}
		else
		{
			LifetimeStats = new LifetimeStatsController().LifetimeStats;
		}
	}

	public override void Log(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = false, bool printStackTrace = false)
	{
		TPSingleton<PlayableUnitManager>.Instance.Log($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogError(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = true)
	{
		TPSingleton<PlayableUnitManager>.Instance.LogError($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogWarning(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = false)
	{
		TPSingleton<PlayableUnitManager>.Instance.LogWarning($"[{UniqueIdentifier}]: {message}", UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public int GetPerkModuleBuffer(string perkId, double moduleIndex, double bufferIndex)
	{
		if (!Perks.TryGetValue(perkId, out var value) || !value.Unlocked)
		{
			return 0;
		}
		return value.GetModuleBuffer(moduleIndex, bufferIndex);
	}

	public int ItemsNumberInSlot(string slotId)
	{
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(slotId, out var result))
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in EquipmentSlots)
		{
			if (result.HasFlag(equipmentSlot.Key))
			{
				num += equipmentSlot.Value.Count((EquipmentSlot slot) => slot.Item != null);
			}
		}
		return num;
	}

	public int AvailableSlotsNbByType(string slotId)
	{
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(slotId, out var result))
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in EquipmentSlots)
		{
			if (result.HasFlag(equipmentSlot.Key))
			{
				num += equipmentSlot.Value.Count;
			}
		}
		return num;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1, bool isDead = false)
	{
		SerializedPlayableUnit serializedPlayableUnit = container as SerializedPlayableUnit;
		base.Deserialize(serializedPlayableUnit.Unit, saveVersion);
		PlayableUnitName = serializedPlayableUnit.Name;
		ArchetypeId = serializedPlayableUnit.ArchetypeId;
		Gender = serializedPlayableUnit.Portrait.Gender;
		IsStartingUnit = serializedPlayableUnit.IsStartingUnit;
		FaceId = serializedPlayableUnit.Portrait.FaceId;
		MovedThisDay = serializedPlayableUnit.MovedThisDay;
		LastTurnHealth = serializedPlayableUnit.LastTurnHealth;
		HelmetDisplayed = serializedPlayableUnit.HelmetDisplayed;
		ActionPointsSpentThisTurn = serializedPlayableUnit.ActionPointsSpentThisTurn;
		MomentumTilesActive = serializedPlayableUnit.MomentumTilesActive;
		TotalMomentumTilesCrossedThisTurn = serializedPlayableUnit.TotalMomentumTilesCrossedThisTurn;
		TilesCrossedThisTurn = serializedPlayableUnit.TilesCrossedThisTurn;
		if (isDead)
		{
			base.State = E_State.Dead;
		}
		RaceDefinition value;
		if (string.IsNullOrEmpty(serializedPlayableUnit.RaceId))
		{
			RaceDefinition = PlayableUnitDatabase.RaceDefinitions["Human"];
		}
		else if (PlayableUnitDatabase.RaceDefinitions.TryGetValue(serializedPlayableUnit.RaceId, out value))
		{
			RaceDefinition = value;
		}
		else
		{
			TPSingleton<PlayableUnitManager>.Instance.LogWarning("Trying to load RaceDefinition " + serializedPlayableUnit.RaceId + " but it wasn't found in Database. Assigning default race definition.");
			RaceDefinition = PlayableUnitDatabase.RaceDefinitions["Human"];
		}
		Level = serializedPlayableUnit.Level;
		LevelUp = new UnitLevelUpController(PlayableUnitDatabase.UnitLevelUpDefinition, serializedPlayableUnit.LevelUp).UnitLevelUp;
		LevelUp.PlayableUnit = this;
		PerksPoints = serializedPlayableUnit.PerksPoints;
		PlayableUnitPerksController = new PlayableUnitPerksController(this);
		UnitLevelUpPoints = new List<UnitLevelUpPoint>();
		foreach (SerializedLevelUpPoint serializedLevelUpPoint in serializedPlayableUnit.SerializedLevelUpPoints)
		{
			UnitLevelUpPoints.Add(new UnitLevelUpPoint(serializedLevelUpPoint));
		}
		Experience = serializedPlayableUnit.Experience;
		ExperienceInCurrentLevel = serializedPlayableUnit.ExperienceInCurrentLevel;
		EquippedWeaponSetIndex = serializedPlayableUnit.EquippedWeaponSetIndex;
		ContextualSkills = new List<TheLastStand.Model.Skill.Skill>();
		foreach (SerializedSkill contextualSkill in serializedPlayableUnit.ContextualSkills)
		{
			ContextualSkills.Add(new SkillController(contextualSkill, this).Skill);
		}
		foreach (string trait in serializedPlayableUnit.Traits)
		{
			if (!PlayableUnitDatabase.UnitTraitDefinitions.TryGetValue(trait, out var value2))
			{
				TPSingleton<PlayableUnitManager>.Instance.LogWarning("Trying to load TraitDefinition " + trait + " but it wasn't found in Database. Skipping it.");
			}
			else
			{
				UnitTraitDefinitions.Add(value2);
			}
		}
		foreach (SerializedEquipmentSlot equipmentSlot2 in serializedPlayableUnit.EquipmentSlots)
		{
			foreach (SerializedItemSlot itemSlot in equipmentSlot2.ItemSlots)
			{
				ItemSlotDefinition.E_ItemSlotId id = equipmentSlot2.Id;
				int num = (EquipmentSlots.ContainsKey(id) ? EquipmentSlots[id].Count : 0);
				int num2 = ((EquippedWeaponSetIndex != num) ? 1 : 0);
				EquipmentSlotView equipmentSlotView = CharacterSheetPanel.EquipmentSlots[id][ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(itemSlot.Id) ? num2 : num];
				EquipmentSlot equipmentSlot;
				try
				{
					equipmentSlot = new EquipmentSlotController(itemSlot, equipmentSlotView, this).EquipmentSlot;
				}
				catch (Database<ItemDatabase>.MissingAssetException arg)
				{
					TPSingleton<InventoryManager>.Instance.LogError($"Could not find equipment {itemSlot.Item.Id} for slot {itemSlot.Id}, this equipment will be skipped.\n{arg}", CLogLevel.DETAILED);
					continue;
				}
				if (!EquipmentSlots.ContainsKey(id))
				{
					EquipmentSlots.Add(id, new List<EquipmentSlot>());
				}
				EquipmentSlots[id].Add(equipmentSlot);
			}
		}
		base.UnitStatsController = new PlayableUnitStatsController(container as SerializedUnitStats, this);
		PerkRerollCount = serializedPlayableUnit.PerkRerollCount;
		PerkTree = new UnitPerkTreeController(TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView, this).UnitPerkTree;
		PerkTree.UnitPerkTreeController.GeneratePerkTree(serializedPlayableUnit.PerkCollections, this, isDead);
	}

	public override void DeserializeAfterInit(ISerializedData container, int saveVersion)
	{
		base.DeserializeAfterInit(container, saveVersion);
		SerializedPlayableUnit serializedPlayableUnit = container as SerializedPlayableUnit;
		if (!EquipmentSlots.ContainsKey(ItemSlotDefinition.E_ItemSlotId.LeftHand))
		{
			BodyParts["Arm_L"].ChangeAdditionalConstraint("Hide", add: true);
		}
		if (serializedPlayableUnit.Stats != null)
		{
			DeserializeStats(serializedPlayableUnit.Stats, saveVersion);
		}
		PlayableUnitStatsController.RefreshEquipmentValues();
		if (UnitView != null)
		{
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.Health);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.Armor);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.Mana);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.ActionPoints);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.MovePoints);
		}
	}

	public override void DeserializeStats(ISerializedData container, int saveVersion)
	{
		PlayableUnitStatsController.PlayableUnitStats.Deserialize(container, saveVersion);
	}

	public override ISerializedData Serialize()
	{
		return new SerializedPlayableUnit((SerializedUnit)base.Serialize())
		{
			Name = PlayableUnitName,
			ArchetypeId = ArchetypeId,
			Portrait = new SerializedPortrait
			{
				Gender = Gender,
				FaceId = FaceId,
				Code = PortraitCodeData.ToString()
			},
			IsStartingUnit = IsStartingUnit,
			LastTurnHealth = LastTurnHealth,
			PerksPoints = PerksPoints,
			SerializedLevelUpPoints = UnitLevelUpPoints.Select((UnitLevelUpPoint o) => (SerializedLevelUpPoint)o.Serialize()).ToList(),
			Experience = Experience,
			ExperienceInCurrentLevel = ExperienceInCurrentLevel,
			ContextualSkills = ContextualSkills.Select((TheLastStand.Model.Skill.Skill o) => (SerializedSkill)o.Serialize()).ToList(),
			Level = Level,
			LevelUp = (SerializedLevelUpBonuses)LevelUp.Serialize(),
			EquippedWeaponSetIndex = EquippedWeaponSetIndex,
			EquipmentSlots = EquipmentSlots.Select((KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> o) => new SerializedEquipmentSlot
			{
				Id = o.Key,
				ItemSlots = o.Value.Select((EquipmentSlot equipSlot) => (SerializedItemSlot)equipSlot.Serialize()).ToList()
			}).ToList(),
			Traits = UnitTraitDefinitions.Select((UnitTraitDefinition o) => o.Id).ToList(),
			PerkCollections = PerkTree.Serialize(),
			NativePerks = (from p in Perks
				where p.Value.IsNative
				select (SerializedPerk)p.Value.Serialize()).ToList(),
			DynamicPerks = (from p in Perks
				where !p.Value.IsNative && !p.Value.IsFromRace && p.Value.PerkTier == null
				select (SerializedPerk)p.Value.Serialize()).ToList(),
			RacePerks = (from p in Perks
				where p.Value.IsFromRace
				select (SerializedPerk)p.Value.Serialize()).ToList(),
			LifetimeStats = (SerializedLifetimeStats)LifetimeStats.Serialize(),
			MovedThisDay = MovedThisDay,
			HelmetDisplayed = HelmetDisplayed,
			ActionPointsSpentThisTurn = ActionPointsSpentThisTurn,
			MomentumTilesActive = MomentumTilesActive,
			TotalMomentumTilesCrossedThisTurn = TotalMomentumTilesCrossedThisTurn,
			TilesCrossedThisTurn = TilesCrossedThisTurn,
			Stats = (SerializedUnitStats)base.UnitStatsController.UnitStats.Serialize(),
			RaceId = RaceDefinition.Id,
			PerkRerollCount = PerkRerollCount
		};
	}
}
