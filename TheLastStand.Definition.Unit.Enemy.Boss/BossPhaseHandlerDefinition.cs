using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossPhaseHandlerDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public static class Actions
		{
			public const string SetPhase = "SetPhase";

			public const string PauseWave = "PauseWave";

			public const string SetHandlerLock = "SetHandlerLock";

			public const string DestroyActor = "DestroyActor";

			public const string SpawnActor = "SpawnActor";

			public const string SetNightProgression = "SetNightProgression";

			public const string EvolutiveLevelArtSetStage = "EvolutiveLevelArtSetStage";

			public const string EvolutiveLevelArtSetActiveCurrentStage = "EvolutiveLevelArtSetActiveCurrentStage";

			public const string ReplaceActors = "ReplaceActors";

			public const string PlayCutscene = "PlayCutscene";

			public const string UnlockSpawnerBossAchievement = "UnlockSpawnerBossAchievement";
		}

		public const string Name = "PhaseHandler";
	}

	public List<ABossPhaseActionDefinition> ActionsDefinitions { get; } = new List<ABossPhaseActionDefinition>();

	public List<IBossPhaseConditionDefinition> ConditionsDefinitions { get; } = new List<IBossPhaseConditionDefinition>();

	public string Id { get; private set; }

	public bool DefaultLockValue { get; private set; }

	public BossPhaseHandlerDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("Conditions");
		if (xElement2 != null)
		{
			foreach (XElement item in xElement2.Elements())
			{
				if (BossPhaseConditionsFactory.BossPhaseConditionDefinitionFromXElement(item, out var bossPhaseContentDefinition))
				{
					ConditionsDefinitions.Add(bossPhaseContentDefinition);
				}
			}
		}
		XElement xElement3 = xElement.Element("Actions");
		if (xElement3 != null)
		{
			foreach (XElement item2 in xElement3.Elements())
			{
				switch (item2.Name.LocalName)
				{
				case "DestroyActor":
					ActionsDefinitions.Add(new DestroyActorPhaseActionDefinition(item2));
					break;
				case "EvolutiveLevelArtSetStage":
					ActionsDefinitions.Add(new EvolutiveLevelArtSetStagePhaseActionDefinition(item2));
					break;
				case "EvolutiveLevelArtSetActiveCurrentStage":
					ActionsDefinitions.Add(new EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition(item2));
					break;
				case "PauseWave":
					ActionsDefinitions.Add(new PauseWavePhaseActionDefinition(item2));
					break;
				case "PlayCutscene":
					ActionsDefinitions.Add(new PlayCutscenePhaseActionDefinition(item2));
					break;
				case "ReplaceActors":
					ActionsDefinitions.Add(new ReplaceActorsPhaseActionDefinition(item2));
					break;
				case "SetHandlerLock":
					ActionsDefinitions.Add(new SetPhaseHandlerLockPhaseActionDefinition(item2));
					break;
				case "SetNightProgression":
					ActionsDefinitions.Add(new SetNightProgressionPhaseActionDefinition(item2));
					break;
				case "SetPhase":
					ActionsDefinitions.Add(new SetPhasePhaseActionDefinition(item2));
					break;
				case "SpawnActor":
					ActionsDefinitions.Add(new SpawnActorPhaseActionDefinition(item2));
					break;
				case "UnlockSpawnerBossAchievement":
					ActionsDefinitions.Add(new UnlockSpawnerBossAchievementPhaseActionDefinition(item2));
					break;
				}
			}
		}
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("IsLockedByDefault");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result))
			{
				DefaultLockValue = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}
