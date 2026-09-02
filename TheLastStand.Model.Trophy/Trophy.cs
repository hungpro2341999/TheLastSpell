using System.Collections.Generic;
using System.Linq;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Trophy;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Database;
using TheLastStand.Definition.Trophy;

namespace TheLastStand.Model.Trophy;

public class Trophy
{
	public class StringToTrophyIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(TrophyDatabase.TrophyDefinitions.Select((TrophyDefinition x) => x.Id));
	}

	public TrophyController TrophyController { get; }

	public TrophyDefinition TrophyDefinition { get; }

	public TrophyConditionController TrophyConditionController { get; private set; }

	public string Name => Localizer.Get("TrophyName_" + TrophyDefinition.Id);

	public Trophy(TrophyDefinition trophyDefinition, TrophyController trophyController)
	{
		TrophyController = trophyController;
		TrophyDefinition = trophyDefinition;
		CreateConditionController();
	}

	private void CreateConditionController()
	{
		TrophyConditionController = TrophyDefinition.Condition.ToString() switch
		{
			"HealthLost" => new HealthLostTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesKilled" => new EnemiesKilledTrophyConditionController(TrophyDefinition.Condition, this), 
			"DefensesLost" => new DefensesLostTrophyConditionController(TrophyDefinition.Condition, this), 
			"UsableUsed" => new UsableUsedTrophyConditionController(TrophyDefinition.Condition, this), 
			"StatusInflicted" => new StatusInflictedTrophyConditionController(TrophyDefinition.Condition, this), 
			"OpportunisticTriggered" => new OpportunisticTriggeredTrophyConditionController(TrophyDefinition.Condition, this), 
			"OpportunismDamageInflicted" => new OpportunismDamageInflictedTrophyConditionController(TrophyDefinition.Condition, this), 
			"NoHealthLost" => new NoHealthLostTrophyConditionController(TrophyDefinition.Condition, this), 
			"BloodyKilledAfterEatingAllies" => new BloodyKilledAfterEatingAlliesTrophyConditionController(TrophyDefinition.Condition, this), 
			"HeroSurroundedByEnemies" => new HeroSurroundedByEnemiesTrophyConditionController(TrophyDefinition.Condition, this), 
			"NightCompleted" => new NightCompletedTrophyConditionController(TrophyDefinition.Condition, this), 
			"NightCompletedXTurnsAfterSpawnEnd" => new NightCompletedXTurnsAfterSpawnEndConditionController(TrophyDefinition.Condition, this), 
			"PerfectPanic" => new PerfectPanicTrophyConditionController(TrophyDefinition.Condition, this), 
			"PunchUsed" => new PunchUsedTrophyConditionController(TrophyDefinition.Condition, this), 
			"JumpOverWallUsed" => new JumpOverWallUsedTrophyConditionController(TrophyDefinition.Condition, this), 
			"HealthRemainingAtMost" => new HealthRemainingAtMostTrophyConditionController(TrophyDefinition.Condition, this), 
			"TilesMovedUsingSkills" => new TilesMovedUsingSkillsTrophyConditionController(TrophyDefinition.Condition, this), 
			"TilesMovedBeforeMomentum" => new TilesMovedBeforeMomentumTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesDamagedByBoomer" => new EnemiesDamagedByBoomerTrophyConditionController(TrophyDefinition.Condition, this), 
			"ManaSpent" => new ManaSpentTrophyConditionController(TrophyDefinition.Condition, this), 
			"HeroDead" => new HeroDeadTrophyConditionController(TrophyDefinition.Condition, this), 
			"NoDodgeTriggered" => new NoDodgeTriggeredTrophyConditionController(TrophyDefinition.Condition, this), 
			"DodgesPerformed" => new DodgesPerformedTrophyConditionController(TrophyDefinition.Condition, this), 
			"SurviveWithFewWalls" => new SurviveWithFewWallsTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesDamaged" => new EnemiesDamagedTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesKilledFromWatchtower" => new EnemiesKilledFromWatchtowerTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesKilledWithoutAttack" => new EnemiesKilledWithoutAttackTrophyConditionController(TrophyDefinition.Condition, this), 
			"SpeedyKilledWithoutDodging" => new SpeedyKilledWithoutDodgingTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesDebuffedSeveralTimesSingleTurn" => new EnemiesDebuffedSeveralTimesSingleTurnTrophyConditionController(TrophyDefinition.Condition, this), 
			"BodyArmorBuffUsed" => new BodyArmorBuffUsedTrophyConditionController(TrophyDefinition.Condition, this), 
			"DamageInflicted" => new DamageInflictedTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesKilledSingleAttack" => new EnemiesKilledSingleAttackTrophyConditionController(TrophyDefinition.Condition, this), 
			"CatapultUsed" => new CatapultUsedTrophyConditionController(TrophyDefinition.Condition, this), 
			"BuildingsLost" => new BuildingsLostTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesKilledByPropagation" => new EnemiesKilledByPropagationTrophyConditionController(TrophyDefinition.Condition, this), 
			"EnemiesKilledByIsolated" => new EnemiesKilledByIsolatedTrophyConditionController(TrophyDefinition.Condition, this), 
			"ArmoredEnemiesDamagedByArmorShredding" => new ArmoredEnemiesDamagedByArmorShreddingTrophyConditionController(TrophyDefinition.Condition, this), 
			"DamageInflictedSingleAttack" => new DamageInflictedSingleAttackTrophyConditionController(TrophyDefinition.Condition, this), 
			"GhostKilledWithoutDebuffing" => new GhostKilledWithoutDebuffingTrophyConditionController(TrophyDefinition.Condition, this), 
			"CriticalsInflictedSingleTurn" => new CriticalsInflictedSingleTurnTrophyConditionController(TrophyDefinition.Condition, this), 
			"FriendlyFire" => new FriendlyFireTrophyConditionController(TrophyDefinition.Condition, this), 
			_ => TrophyConditionController, 
		};
	}
}
