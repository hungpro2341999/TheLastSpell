using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class ApocalypseEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Consts
	{
		public const string AddAffixFlag = "AddAffixFlag";

		public const string AddEnemiesStatModifierFromTurn = "AddEnemiesStatModifierFromTurn";

		public const string AddSkillProgressionFlag = "AddSkillProgressionFlag";

		public const string EnemiesStatBaseValueModifier = "EnemiesStatBaseValueModifier";

		public const string ForbidBuildingCategoryAroundMagicCircle = "ForbidBuildingCategoryAroundMagicCircle";

		public const string GenerateFogSpawners = "GenerateFogSpawners";

		public const string GenerateMalusAffixes = "GenerateMalusAffixes";

		public const string IncreaseEnemiesNumber = "IncreaseEnemiesNumber";

		public const string IncreaseEnemiesProgressionOffset = "IncreaseEnemiesProgressionOffset";

		public const string IncreasePrices = "IncreasePrices";

		public const string IncreaseStartingFogDensity = "IncreaseStartingFogDensity";

		public const string IncreaseDailyFogUpdateFrequency = "IncreaseDailyFogUpdateFrequency";

		public const string ModifyBuildingsDeadZoneRange = "ModifyBuildingsDeadZoneRange";

		public const string ModifyEnemiesInjuryStage = "ModifyEnemiesInjuryStage";

		public const string ModifyInnSlotRecruitmentLevel = "ModifyInnSlotRecruitmentLevel";

		public const string ModifyMagicCircleStartingHealth = "ModifyMagicCircleStartingHealth";

		public const string MultiplyEnemyUnitSpawnWaveWeight = "MultiplyEnemyUnitSpawnWaveWeight";

		public const string MultiplyPanicGain = "MultiplyPanicGain";

		public const string PlayableUnitBlockLineOfSight = "PlayableUnitBlockLineOfSight";

		public const string RemoveStartingPlayableUnit = "RemoveStartingPlayableUnit";

		public const string SetBuildingsNotDemolishable = "SetBuildingsNotDemolishable";
	}

	public ApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
