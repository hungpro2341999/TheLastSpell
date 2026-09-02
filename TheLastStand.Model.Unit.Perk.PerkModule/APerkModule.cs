using System.Collections.Generic;
using System.Linq;
using TPLib.Log;
using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Perk.PerkCondition;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Unit.Perk.PerkCondition;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.Serialization.Perk;
using UnityEngine;

namespace TheLastStand.Model.Unit.Perk.PerkModule;

public abstract class APerkModule : ISerializable, IDeserializable
{
	public bool IsActive => PerkConditions.All((APerkCondition x) => x.IsValid());

	public bool WasActiveOnLastRefresh { get; set; }

	public Perk Perk { get; private set; }

	public List<APerkCondition> PerkConditions { get; private set; }

	public List<APerkEffect> PerkEffects { get; private set; }

	public List<TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent> PerkEvents { get; private set; }

	public APerkModuleDefinition PerkModuleDefinition { get; private set; }

	public CastSkillEffect CastSkillEffect => PerkEffects.FirstOrDefault((APerkEffect x) => x is CastSkillEffect) as CastSkillEffect;

	public APerkModule(APerkModuleDefinition perkModuleDefinition, Perk perk)
	{
		PerkModuleDefinition = perkModuleDefinition;
		Perk = perk;
		GenerateConditions();
		GenerateEvents();
		GenerateEffects();
	}

	public virtual void OnUnlock(bool onLoad)
	{
	}

	public virtual void Lock(bool onLoad)
	{
	}

	public virtual void ResetDynamicData()
	{
	}

	public void TriggerEffects(PerkDataContainer data)
	{
		Perk.Owner.Log("Perk " + Perk.PerkDefinition.Id + " Triggered.", CLogLevel.DETAILED);
		foreach (APerkEffect perkEffect in PerkEffects)
		{
			if (perkEffect.PerkDataConditions.IsValid(data))
			{
				perkEffect.APerkEffectController.Trigger(data);
			}
		}
	}

	public bool TryTriggerEffects(PerkDataContainer data)
	{
		if (IsActive)
		{
			TriggerEffects(data);
		}
		return IsActive;
	}

	protected void GenerateConditions()
	{
		PerkConditions = new List<APerkCondition>(PerkModuleDefinition.PerkConditionDefinitions.Count);
		foreach (APerkConditionDefinition perkConditionDefinition in PerkModuleDefinition.PerkConditionDefinitions)
		{
			if (!(perkConditionDefinition is IsTrueConditionDefinition aPerkConditionDefinition))
			{
				if (perkConditionDefinition is IsFalseConditionDefinition aPerkConditionDefinition2)
				{
					PerkConditions.Add(new IsFalseCondition(aPerkConditionDefinition2, this));
				}
				else
				{
					CLoggerManager.Log($"Tried to Generate a PerkCondition that isn't implemented : {perkConditionDefinition}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "APerkModule");
				}
			}
			else
			{
				PerkConditions.Add(new IsTrueCondition(aPerkConditionDefinition, this));
			}
		}
	}

	protected void GenerateEvents()
	{
		PerkEvents = new List<TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent>(PerkModuleDefinition.PerkEventDefinitions.Count);
		foreach (PerkEventDefinition perkEventDefinition in PerkModuleDefinition.PerkEventDefinitions)
		{
			PerkEvents.Add(new TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent(perkEventDefinition, this));
		}
	}

	protected void GenerateEffects()
	{
		PerkEffects = new List<APerkEffect>(PerkModuleDefinition.PerkEffectDefinitions.Count);
		foreach (APerkEffectDefinition perkEffectDefinition in PerkModuleDefinition.PerkEffectDefinitions)
		{
			if (!(perkEffectDefinition is PermanentBaseStatModifierEffectDefinition aPerkEffectDefinition))
			{
				if (!(perkEffectDefinition is StatModifierEffectDefinition aPerkEffectDefinition2))
				{
					if (!(perkEffectDefinition is StatLockerEffectDefinition aPerkEffectDefinition3))
					{
						if (!(perkEffectDefinition is DynamicStatsModifierEffectDefinition aPerkEffectDefinition4))
						{
							if (!(perkEffectDefinition is ComputationStatLockerEffectDefinition aPerkEffectDefinition5))
							{
								if (!(perkEffectDefinition is SkillModifierEffectDefinition aPerkEffectDefinition6))
								{
									if (!(perkEffectDefinition is AddSkillEffectDefinition aPerkEffectDefinition7))
									{
										if (!(perkEffectDefinition is AddPerkSkillEffectDefinition aPerkEffectDefinition8))
										{
											if (!(perkEffectDefinition is LockSkillEffectDefinition aPerkEffectDefinition9))
											{
												if (!(perkEffectDefinition is AllowDiagonalPropagationEffectDefinition aPerkEffectDefinition10))
												{
													if (!(perkEffectDefinition is UnlockContextualSkillEffectDefinition aPerkEffectDefinition11))
													{
														if (!(perkEffectDefinition is SwapContextualSkillEffectDefinition aPerkEffectDefinition12))
														{
															if (!(perkEffectDefinition is ResetBufferEffectDefinition aPerkEffectDefinition13))
															{
																if (!(perkEffectDefinition is RestoreStatEffectDefinition aPerkEffectDefinition14))
																{
																	if (!(perkEffectDefinition is ApplyStatusEffectDefinition aPerkEffectDefinition15))
																	{
																		if (!(perkEffectDefinition is CastSkillEffectDefinition aPerkEffectDefinition16))
																		{
																			if (!(perkEffectDefinition is DealDamageEffectDefinition aPerkEffectDefinition17))
																			{
																				if (!(perkEffectDefinition is ModifyDefensesDamageEffectDefinition aPerkEffectDefinition18))
																				{
																					if (!(perkEffectDefinition is GetAdditionalExperienceEffectDefinition aPerkEffectDefinition19))
																					{
																						if (!(perkEffectDefinition is EquipmentSlotModifierEffectDefinition aPerkEffectDefinition20))
																						{
																							if (!(perkEffectDefinition is RestoreUsesEffectDefinition aPerkEffectDefinition21))
																							{
																								if (!(perkEffectDefinition is ReplacePerkEffectDefinition aPerkEffectDefinition22))
																								{
																									if (!(perkEffectDefinition is AttackDataModifierEffectDefinition aPerkEffectDefinition23))
																									{
																										if (!(perkEffectDefinition is ReplaceItemSkillEffectDefinition aPerkEffectDefinition24))
																										{
																											if (!(perkEffectDefinition is StatBoundaryModifierEffectDefinition aPerkEffectDefinition25))
																											{
																												if (!(perkEffectDefinition is MomentumModifierEffectDefinition aPerkEffectDefinition26))
																												{
																													if (perkEffectDefinition is ModifyContextualSkillOverallUsesEffectDefinition aPerkEffectDefinition27)
																													{
																														PerkEffects.Add(new ModifyContextualSkillOverallUsesEffectController(aPerkEffectDefinition27, this).PerkEffect);
																													}
																													else
																													{
																														CLoggerManager.Log($"Tried to Generate a PerkEffect that isn't implemented : {perkEffectDefinition}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "APerkModule");
																													}
																												}
																												else
																												{
																													PerkEffects.Add(new MomentumModifierEffectController(aPerkEffectDefinition26, this).PerkEffect);
																												}
																											}
																											else
																											{
																												PerkEffects.Add(new StatBoundaryModifierEffectController(aPerkEffectDefinition25, this).PerkEffect);
																											}
																										}
																										else
																										{
																											PerkEffects.Add(new ReplaceItemSkillEffectController(aPerkEffectDefinition24, this).PerkEffect);
																										}
																									}
																									else
																									{
																										PerkEffects.Add(new AttackDataModifierEffectController(aPerkEffectDefinition23, this).PerkEffect);
																									}
																								}
																								else
																								{
																									PerkEffects.Add(new ReplacePerkEffectController(aPerkEffectDefinition22, this).PerkEffect);
																								}
																							}
																							else
																							{
																								PerkEffects.Add(new RestoreUsesEffectController(aPerkEffectDefinition21, this).PerkEffect);
																							}
																						}
																						else
																						{
																							PerkEffects.Add(new EquipmentSlotModifierEffectController(aPerkEffectDefinition20, this).PerkEffect);
																						}
																					}
																					else
																					{
																						PerkEffects.Add(new GetAdditionalExperienceEffectController(aPerkEffectDefinition19, this).PerkEffect);
																					}
																				}
																				else
																				{
																					PerkEffects.Add(new ModifyDefensesDamageEffectController(aPerkEffectDefinition18, this).PerkEffect);
																				}
																			}
																			else
																			{
																				PerkEffects.Add(new DealDamageEffectController(aPerkEffectDefinition17, this).PerkEffect);
																			}
																		}
																		else
																		{
																			PerkEffects.Add(new CastSkillEffectController(aPerkEffectDefinition16, this).PerkEffect);
																		}
																	}
																	else
																	{
																		PerkEffects.Add(new ApplyStatusEffectController(aPerkEffectDefinition15, this).PerkEffect);
																	}
																}
																else
																{
																	PerkEffects.Add(new RestoreStatEffectController(aPerkEffectDefinition14, this).PerkEffect);
																}
															}
															else
															{
																PerkEffects.Add(new ResetBufferEffectController(aPerkEffectDefinition13, this).PerkEffect);
															}
														}
														else
														{
															PerkEffects.Add(new SwapContextualSkillEffectController(aPerkEffectDefinition12, this).PerkEffect);
														}
													}
													else
													{
														PerkEffects.Add(new UnlockContextualSkillEffectController(aPerkEffectDefinition11, this).PerkEffect);
													}
												}
												else
												{
													PerkEffects.Add(new AllowDiagonalPropagationEffectController(aPerkEffectDefinition10, this).PerkEffect);
												}
											}
											else
											{
												PerkEffects.Add(new LockSkillEffectController(aPerkEffectDefinition9, this).PerkEffect);
											}
										}
										else
										{
											PerkEffects.Add(new AddPerkSkillEffectController(aPerkEffectDefinition8, this).PerkEffect);
										}
									}
									else
									{
										PerkEffects.Add(new AddSkillEffectController(aPerkEffectDefinition7, this).PerkEffect);
									}
								}
								else
								{
									PerkEffects.Add(new SkillModifierEffectController(aPerkEffectDefinition6, this).PerkEffect);
								}
							}
							else
							{
								PerkEffects.Add(new ComputationStatLockerEffectController(aPerkEffectDefinition5, this).PerkEffect);
							}
						}
						else
						{
							PerkEffects.Add(new DynamicStatsModifierEffectController(aPerkEffectDefinition4, this).PerkEffect);
						}
					}
					else
					{
						PerkEffects.Add(new StatLockerEffectController(aPerkEffectDefinition3, this).PerkEffect);
					}
				}
				else
				{
					PerkEffects.Add(new StatModifierEffectController(aPerkEffectDefinition2, this).PerkEffect);
				}
			}
			else
			{
				PerkEffects.Add(new PermanentBaseStatModifierEffectController(aPerkEffectDefinition, this).PerkEffect);
			}
		}
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedModule();
	}

	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
	}
}
