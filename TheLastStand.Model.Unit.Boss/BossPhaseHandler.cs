using System.Collections.Generic;
using TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Controller.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Unit.Boss;

public class BossPhaseHandler : ISerializable, IDeserializable
{
	public bool IsLocked;

	public List<ABossPhaseActionController> Actions { get; } = new List<ABossPhaseActionController>();

	public List<ABossPhaseConditionController> Conditions { get; } = new List<ABossPhaseConditionController>();

	public BossPhaseHandlerDefinition BossPhaseHandlerDefinition { get; }

	public string BossPhaseParentId { get; }

	public BossPhaseHandler(BossPhaseHandlerDefinition bossPhaseHandlerDefinition, string bossPhaseParentId)
	{
		BossPhaseHandlerDefinition = bossPhaseHandlerDefinition;
		BossPhaseParentId = bossPhaseParentId;
		foreach (ABossPhaseActionDefinition actionsDefinition in bossPhaseHandlerDefinition.ActionsDefinitions)
		{
			if (!(actionsDefinition is DestroyActorPhaseActionDefinition destroyActorPhaseActionDefinition))
			{
				if (!(actionsDefinition is EvolutiveLevelArtSetStagePhaseActionDefinition aBossPhaseActionDefinition))
				{
					if (!(actionsDefinition is EvolutiveLevelArtSetActiveCurrentStagePhaseActionDefinition aBossPhaseActionDefinition2))
					{
						if (!(actionsDefinition is PauseWavePhaseActionDefinition pauseWavePhaseActionDefinition))
						{
							if (!(actionsDefinition is PlayCutscenePhaseActionDefinition aBossPhaseActionDefinition3))
							{
								if (!(actionsDefinition is ReplaceActorsPhaseActionDefinition aBossPhaseActionDefinition4))
								{
									if (!(actionsDefinition is SetNightProgressionPhaseActionDefinition aBossPhaseActionDefinition5))
									{
										if (!(actionsDefinition is SetPhaseHandlerLockPhaseActionDefinition setPhaseHandlerLockPhaseAction))
										{
											if (!(actionsDefinition is SetPhasePhaseActionDefinition setPhasePhaseActionDefinition))
											{
												if (!(actionsDefinition is SpawnActorPhaseActionDefinition spawnActorPhaseActionDefinition))
												{
													if (actionsDefinition is UnlockSpawnerBossAchievementPhaseActionDefinition unlockSpawnerBossAchievementPhaseActionDefinition)
													{
														Actions.Add(new UnlockSpawnerBossAchievementPhaseActionController(unlockSpawnerBossAchievementPhaseActionDefinition, this, Actions.Count));
													}
												}
												else
												{
													Actions.Add(new SpawnActorPhaseActionController(spawnActorPhaseActionDefinition, this, Actions.Count));
												}
											}
											else
											{
												Actions.Add(new SetPhasePhaseActionController(setPhasePhaseActionDefinition, this, Actions.Count));
											}
										}
										else
										{
											Actions.Add(new SetPhaseHandlerLockPhaseActionController(setPhaseHandlerLockPhaseAction, this, Actions.Count));
										}
									}
									else
									{
										Actions.Add(new SetNightProgressionPhaseActionController(aBossPhaseActionDefinition5, this, Actions.Count));
									}
								}
								else
								{
									Actions.Add(new ReplaceActorsPhaseActionController(aBossPhaseActionDefinition4, this, Actions.Count));
								}
							}
							else
							{
								Actions.Add(new PlayCutscenePhaseActionController(aBossPhaseActionDefinition3, this, Actions.Count));
							}
						}
						else
						{
							Actions.Add(new PauseWavePhaseActionController(pauseWavePhaseActionDefinition, this, Actions.Count));
						}
					}
					else
					{
						Actions.Add(new EvolutiveLevelArtSetActiveCurrentStagePhaseActionController(aBossPhaseActionDefinition2, this, Actions.Count));
					}
				}
				else
				{
					Actions.Add(new EvolutiveLevelArtSetStagePhaseActionController(aBossPhaseActionDefinition, this, Actions.Count));
				}
			}
			else
			{
				Actions.Add(new DestroyActorPhaseActionController(destroyActorPhaseActionDefinition, this, Actions.Count));
			}
		}
		foreach (IBossPhaseConditionDefinition conditionsDefinition in bossPhaseHandlerDefinition.ConditionsDefinitions)
		{
			if (BossPhaseConditionsFactory.BossPhaseConditionControllerFromDefinition(conditionsDefinition, out var bossPhaseConditionController))
			{
				Conditions.Add(bossPhaseConditionController);
			}
		}
	}

	public void Init()
	{
		IsLocked = BossPhaseHandlerDefinition.DefaultLockValue;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedBossPhaseHandler serializedBossPhaseHandler = (SerializedBossPhaseHandler)container;
		IsLocked = serializedBossPhaseHandler.IsLocked;
	}

	public ISerializedData Serialize()
	{
		return new SerializedBossPhaseHandler
		{
			Id = BossPhaseHandlerDefinition.Id,
			IsLocked = IsLocked
		};
	}
}
