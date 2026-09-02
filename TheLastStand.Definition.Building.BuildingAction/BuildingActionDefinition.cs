using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class BuildingActionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public static class Ids
		{
			public const string FillGauge = "FillGauge";

			public const string Heal = "Heal";

			public const string HealMana = "HealMana";

			public const string Scavenge = "Scavenge";

			public const string GainGold = "GainGold";

			public const string GainMaterials = "GainMaterials";

			public const string RepelFog = "RepelFog";

			public const string RevealWaveEnemiesRatio = "RevealWaveEnemiesRatio";

			public const string RerollWave = "RerollWave";

			public const string UpgradeStat = "UpgradeStat";
		}
	}

	private int workersCost;

	public List<BuildingActionEffectDefinition> BuildingActionEffectDefinition { get; private set; }

	public CastFxDefinition CastFxDefinition { get; private set; }

	public bool ContainsRepelFogEffect { get; private set; }

	public string Id { get; private set; }

	public string LoreDescription => string.Empty;

	public PhaseStates PhaseStates { get; } = new PhaseStates(PhaseStates.E_PhaseState.Available, PhaseStates.E_PhaseState.Available, PhaseStates.E_PhaseState.Available);

	public string Name => Localizer.Get("BuildingActionName_" + Id);

	public int UsesPerTurnCount { get; private set; } = -1;

	public int WorkersCost
	{
		get
		{
			if (WorkersExpression != null)
			{
				if (!(ApplicationManager.Application.State.GetName() == "Game"))
				{
					return -1;
				}
				return WorkersExpression.EvalToInt(TPSingleton<GameManager>.Instance);
			}
			return workersCost;
		}
	}

	public Node WorkersExpression { get; private set; }

	public BuildingActionDefinition(XContainer container)
		: base(container)
	{
	}

	public string GetDescription(int unitsThreshold = -1, int productionValue = 0)
	{
		return Localizer.Format("BuildingActionDescription_" + Id, GetArguments(unitsThreshold, productionValue));
	}

	public virtual BuildingActionDefinition Clone()
	{
		return MemberwiseClone() as BuildingActionDefinition;
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute == null)
		{
			Debug.LogError("The skill has no ID !");
			return;
		}
		Id = xAttribute.Value;
		if (xElement.Element("UsesPerTurnCount") != null)
		{
			if (int.TryParse(xElement.Element("UsesPerTurnCount").Value, out var result))
			{
				UsesPerTurnCount = result;
			}
			else
			{
				Debug.LogError("Error while parsing UsesPerTurnCount parameter of building action " + Id + " !");
			}
		}
		XElement xElement2 = xElement.Element("PhaseStates");
		if (xElement2 != null)
		{
			XElement xElement3 = xElement2.Element("Production");
			if (!xElement3.IsNullOrEmpty())
			{
				if (!Enum.TryParse<PhaseStates.E_PhaseState>(xElement3.Value, out var result2))
				{
					Debug.LogError("BuildingActionDefinition " + Id + " PhaseStates Production must be a valid E_PhaseState!");
					return;
				}
				PhaseStates.ProductionState = result2;
			}
			XElement xElement4 = xElement2.Element("Deployment");
			if (!xElement4.IsNullOrEmpty())
			{
				if (!Enum.TryParse<PhaseStates.E_PhaseState>(xElement4.Value, out var result3))
				{
					Debug.LogError("BuildingActionDefinition " + Id + " PhaseStates Deployment must be a valid E_PhaseState!");
					return;
				}
				PhaseStates.DeploymentState = result3;
			}
			XElement xElement5 = xElement2.Element("Night");
			if (!xElement5.IsNullOrEmpty())
			{
				if (!Enum.TryParse<PhaseStates.E_PhaseState>(xElement5.Value, out var result4))
				{
					Debug.LogError("BuildingActionDefinition " + Id + "PhaseStates Night must be a valid E_PhaseState!");
					return;
				}
				PhaseStates.NightState = result4;
			}
		}
		XElement xElement6 = xElement.Element("WorkersCost");
		if (!string.IsNullOrEmpty(xElement6?.Value))
		{
			if (int.TryParse(xElement6.Value, out var result5))
			{
				workersCost = result5;
			}
			else
			{
				WorkersExpression = Parser.Parse(xElement6.Value);
			}
		}
		XElement xElement7 = xElement.Element("ActionEffects");
		if (xElement7 != null)
		{
			BuildingActionEffectDefinition = new List<BuildingActionEffectDefinition>();
			foreach (XElement item in xElement7.Elements())
			{
				BuildingActionEffectDefinition buildingActionEffectDefinition = item.Name.LocalName switch
				{
					"FillGauge" => new FillGaugeBuildingActionEffectDefinition(item, this), 
					"Heal" => new HealBuildingActionEffectDefinition(item, this), 
					"HealMana" => new HealManaBuildingActionEffectDefinition(item, this), 
					"Scavenge" => new ScavengeBuildingActionEffectDefinition(item, this), 
					"GainGold" => new GainGoldBuildingActionEffectDefinition(item, this), 
					"GainMaterials" => new GainMaterialsBuildingActionEffectDefinition(item, this), 
					"RepelFog" => new RepelFogBuildingActionEffectDefinition(item, this), 
					"RevealWaveEnemiesRatio" => new RevealDangerIndicatorsBuildingActionEffectDefinition(item, this), 
					"RerollWave" => new RerollWaveBuildingActionEffectDefinition(item, this), 
					"UpgradeStat" => new UpgradeStatBuildingActionEffectDefinition(item, this), 
					_ => null, 
				};
				if (buildingActionEffectDefinition is RepelFogBuildingActionEffectDefinition)
				{
					ContainsRepelFogEffect = true;
				}
				BuildingActionEffectDefinition.Add(buildingActionEffectDefinition);
			}
		}
		XElement xElement8 = xElement.Element("CastFXs");
		if (xElement8 != null)
		{
			CastFxDefinition = new CastFxDefinition(xElement8);
		}
	}

	protected object[] GetArguments(int unitsThreshold = -1, int productionValue = 0)
	{
		List<object> list = new List<object>();
		foreach (BuildingActionEffectDefinition item in BuildingActionEffectDefinition)
		{
			if (!(item is FillGaugeBuildingActionEffectDefinition fillGaugeBuildingActionEffectDefinition))
			{
				if (!(item is GainGoldBuildingActionEffectDefinition gainGoldBuildingActionEffectDefinition))
				{
					if (!(item is GainMaterialsBuildingActionEffectDefinition gainMaterialsBuildingActionEffectDefinition))
					{
						if (!(item is HealBuildingActionEffectDefinition healBuildingActionEffectDefinition))
						{
							if (!(item is HealManaBuildingActionEffectDefinition healManaBuildingActionEffectDefinition))
							{
								if (!(item is RepelFogBuildingActionEffectDefinition repelFogBuildingActionEffectDefinition))
								{
									if (item is ScavengeBuildingActionEffectDefinition scavengeBuildingActionEffectDefinition)
									{
										list.Add(scavengeBuildingActionEffectDefinition.GainGold);
										list.Add(scavengeBuildingActionEffectDefinition.GainMaterials);
										list.Add(scavengeBuildingActionEffectDefinition.GainDamnedSouls);
										list.Add(scavengeBuildingActionEffectDefinition.CreateItemDefinitions.Count);
									}
								}
								else
								{
									list.Add(repelFogBuildingActionEffectDefinition.Amount);
								}
							}
							else
							{
								list.Add(healManaBuildingActionEffectDefinition.Amount);
							}
						}
						else
						{
							list.Add(healBuildingActionEffectDefinition.Amount);
						}
					}
					else
					{
						list.Add(gainMaterialsBuildingActionEffectDefinition.GainMaterials);
					}
				}
				else
				{
					list.Add(gainGoldBuildingActionEffectDefinition.GainGold);
				}
			}
			else
			{
				list.Add(GetFillEffectAmount(fillGaugeBuildingActionEffectDefinition, unitsThreshold, productionValue));
			}
		}
		return list.ToArray();
	}

	private object GetFillEffectAmount(FillGaugeBuildingActionEffectDefinition fillGaugeBuildingActionEffectDefinition, int unitsThreshold, int productionValue)
	{
		return (unitsThreshold > 0) ? (fillGaugeBuildingActionEffectDefinition.Amount / unitsThreshold * productionValue) : 0;
	}
}
