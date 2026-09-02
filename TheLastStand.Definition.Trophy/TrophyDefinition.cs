using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Trophy;

public class TrophyDefinition : TheLastStand.Framework.Serialization.Definition
{
	private uint damnedSoulsEarnedBase;

	public string Id { get; private set; }

	public bool IgnoreGem { get; private set; }

	public bool IsLostOnDefeat { get; private set; }

	public uint DamnedSoulsEarned
	{
		get
		{
			uint num = damnedSoulsEarnedBase;
			uint num2 = TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num2 += TPSingleton<GlyphManager>.Instance.DamnedSoulsPercentageModifier;
			}
			float num3 = 1f + (float)num2 / 100f;
			return (uint)((float)num * num3);
		}
	}

	public string TrophyToOverride { get; private set; }

	public TrophyConditionDefinition Condition { get; private set; }

	public TrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Id = xElement.Attribute("Id").Value;
		XAttribute xAttribute = xElement.Attribute("LostOnDefeat");
		XElement xElement2 = xElement.Element("DamnedSoulsEarned");
		XElement xElement3 = xElement.Element("OverrideTrophy");
		XElement xElement4 = xElement.Element("IgnoreGem");
		if (xAttribute != null)
		{
			IsLostOnDefeat = bool.Parse(xAttribute.Value);
		}
		if (xElement4 != null)
		{
			IgnoreGem = true;
		}
		if (xElement2 != null)
		{
			if (!uint.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("Trophy Definition " + Id + "'s DamnedSoulsEarned " + HasAnInvalid("uint", xElement2.Value), LogType.Error);
			}
			damnedSoulsEarnedBase = result;
		}
		else
		{
			CLoggerManager.Log("A TrophyDefinition should have a DamnedSoulsEarned Element please add it to : " + Id, LogType.Error);
			damnedSoulsEarnedBase = 0u;
		}
		if (xElement3 != null)
		{
			TrophyToOverride = xElement3.Value;
		}
		foreach (XElement item in xElement.Elements())
		{
			Condition = item.Name.LocalName switch
			{
				"HealthLost" => new HealthLostTrophyDefinition(item), 
				"EnemiesKilled" => new EnemiesKilledTrophyDefinition(item), 
				"DefensesLost" => new DefensesLostTrophyDefinition(item), 
				"UsableUsed" => new UsableUsedTrophyDefinition(item), 
				"StatusInflicted" => new StatusInflictedTrophyDefinition(item), 
				"OpportunisticTriggered" => new OpportunisticTriggeredTrophyDefinition(item), 
				"OpportunismDamageInflicted" => new OpportunismDamageInflictedTrophyDefinition(item), 
				"NoHealthLost" => new NoHealthLostTrophyDefinition(item), 
				"BloodyKilledAfterEatingAllies" => new BloodyKilledAfterEatingAlliesTrophyDefinition(item), 
				"HeroSurroundedByEnemies" => new HeroSurroundedByEnemiesTrophyDefinition(item), 
				"NightCompleted" => new NightCompletedTrophyDefinition(item), 
				"NightCompletedXTurnsAfterSpawnEnd" => new NightCompletedXTurnsAfterSpawnEndTrophyDefinition(item), 
				"PerfectPanic" => new PerfectPanicTrophyDefinition(item), 
				"PunchUsed" => new PunchUsedTrophyDefinition(item), 
				"JumpOverWallUsed" => new JumpOverWallUsedTrophyDefinition(item), 
				"HealthRemainingAtMost" => new HealthRemainingAtMostTrophyDefinition(item), 
				"TilesMovedUsingSkills" => new TilesMovedUsingSkillsTrophyDefinition(item), 
				"TilesMovedBeforeMomentum" => new TilesMovedBeforeMomentumTrophyDefinition(item), 
				"EnemiesDamagedByBoomer" => new EnemiesDamagedByBoomerTrophyDefinition(item), 
				"ManaSpent" => new ManaSpentTrophyDefinition(item), 
				"HeroDead" => new HeroDeadTrophyDefinition(item), 
				"NoDodgeTriggered" => new NoDodgeTriggeredTrophyDefinition(item), 
				"DodgesPerformed" => new DodgesPerformedTrophyDefinition(item), 
				"SurviveWithFewWalls" => new SurviveWithFewWallsTrophyDefinition(item), 
				"EnemiesDamaged" => new EnemiesDamagedTrophyDefinition(item), 
				"EnemiesKilledFromWatchtower" => new EnemiesKilledFromWatchtowerTrophyDefinition(item), 
				"EnemiesKilledWithoutAttack" => new EnemiesKilledWithoutAttackTrophyDefinition(item), 
				"SpeedyKilledWithoutDodging" => new SpeedyKilledWithoutDodgingTrophyDefinition(item), 
				"EnemiesDebuffedSeveralTimesSingleTurn" => new EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition(item), 
				"BodyArmorBuffUsed" => new BodyArmorBuffUsedTrophyDefinition(item), 
				"DamageInflicted" => new DamageInflictedTrophyDefinition(item), 
				"EnemiesKilledSingleAttack" => new EnemiesKilledSingleAttackTrophyDefinition(item), 
				"CatapultUsed" => new CatapultUsedTrophyDefinition(item), 
				"BuildingsLost" => new BuildingsLostTrophyDefinition(item), 
				"EnemiesKilledByPropagation" => new EnemiesKilledByPropagationTrophyDefinition(item), 
				"EnemiesKilledByIsolated" => new EnemiesKilledByIsolatedTrophyDefinition(item), 
				"ArmoredEnemiesDamagedByArmorShredding" => new ArmoredEnemiesDamagedByArmorShreddingTrophyDefinition(item), 
				"DamageInflictedSingleAttack" => new DamageInflictedSingleAttackTrophyDefinition(item), 
				"GhostKilledWithoutDebuffing" => new GhostKilledWithoutDebuffingTrophyDefinition(item), 
				"CriticalsInflictedSingleTurn" => new CriticalsInflictedSingleTurnTrophyDefinition(item), 
				"FriendlyFire" => new FriendlyFireTrophyDefinition(item), 
				_ => Condition, 
			};
		}
	}
}
