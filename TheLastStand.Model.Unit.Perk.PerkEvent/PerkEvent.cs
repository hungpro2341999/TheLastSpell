using System.Collections.Generic;
using TPLib.Log;
using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkDataCondition;
using TheLastStand.Model.Unit.Perk.PerkModule;
using UnityEngine;

namespace TheLastStand.Model.Unit.Perk.PerkEvent;

public class PerkEvent
{
	public APerkModule PerkModule { get; private set; }

	public List<APerkAction> PerkActions { get; private set; }

	public PerkDataConditions PerkDataConditions { get; private set; }

	public PerkEventDefinition PerkEventDefinition { get; private set; }

	public PerkEvent(PerkEventDefinition definition, APerkModule perkModule)
	{
		PerkModule = perkModule;
		PerkEventDefinition = definition;
		GeneratePerkActions();
		PerkDataConditions = new PerkDataConditions(PerkEventDefinition.PerkDataConditionsDefinition, PerkModule.Perk);
	}

	public void TryTrigger(PerkDataContainer data)
	{
		if (!PerkDataConditions.IsValid(data) || !PerkModule.Perk.Owner.IsInWorld)
		{
			return;
		}
		foreach (APerkAction perkAction in PerkActions)
		{
			perkAction.PerkActionController.Trigger(data);
		}
	}

	public void GeneratePerkActions()
	{
		PerkActions = new List<APerkAction>(PerkEventDefinition.PerkActionDefinitions.Count);
		foreach (APerkActionDefinition perkActionDefinition in PerkEventDefinition.PerkActionDefinitions)
		{
			if (!(perkActionDefinition is SetBufferDefinition definition))
			{
				if (!(perkActionDefinition is IncreaseBufferDefinition definition2))
				{
					if (!(perkActionDefinition is DecreaseBufferDefinition definition3))
					{
						if (!(perkActionDefinition is TriggerEffectsDefinition definition4))
						{
							if (!(perkActionDefinition is TriggerEffectsOnAllAttackDataDefinition definition5))
							{
								if (!(perkActionDefinition is InstantiateStatEffectDisplayDefinition definition6))
								{
									if (!(perkActionDefinition is InstantiateRestoreEffectDisplayDefinition definition7))
									{
										if (!(perkActionDefinition is InstantiateBuffEffectDisplayDefinition definition8))
										{
											if (!(perkActionDefinition is RefreshPerkActivationFeedbackDefinition definition9))
											{
												if (!(perkActionDefinition is ForbidSkillUndoDefinition definition10))
												{
													if (!(perkActionDefinition is RefillGaugeDefinition definition11))
													{
														if (perkActionDefinition is LogPerkActionDefinition definition12)
														{
															PerkActions.Add(new LogPerkActionController(definition12, this).PerkAction);
														}
														else
														{
															CLoggerManager.Log($"Unimplemented PerkActionDefinition : {perkActionDefinition}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkEvent");
														}
													}
													else
													{
														PerkActions.Add(new RefillGaugeController(definition11, this).PerkAction);
													}
												}
												else
												{
													PerkActions.Add(new ForbidSkillUndoController(definition10, this).PerkAction);
												}
											}
											else
											{
												PerkActions.Add(new RefreshPerkActivationFeedbackController(definition9, this).PerkAction);
											}
										}
										else
										{
											PerkActions.Add(new InstantiateBuffEffectDisplayController(definition8, this).PerkAction);
										}
									}
									else
									{
										PerkActions.Add(new InstantiateRestoreEffectDisplayController(definition7, this).PerkAction);
									}
								}
								else
								{
									PerkActions.Add(new InstantiateStatEffectDisplayController(definition6, this).PerkAction);
								}
							}
							else
							{
								PerkActions.Add(new TriggerEffectsOnAllAttackDataController(definition5, this).PerkAction);
							}
						}
						else
						{
							PerkActions.Add(new TriggerEffectsController(definition4, this).PerkAction);
						}
					}
					else
					{
						PerkActions.Add(new DecreaseBufferController(definition3, this).PerkAction);
					}
				}
				else
				{
					PerkActions.Add(new IncreaseBufferController(definition2, this).PerkAction);
				}
			}
			else
			{
				PerkActions.Add(new SetBufferController(definition, this).PerkAction);
			}
		}
	}
}
