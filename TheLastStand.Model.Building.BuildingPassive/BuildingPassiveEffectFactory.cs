using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Model.Building.BuildingPassive;

public static class BuildingPassiveEffectFactory
{
	public class SerializedBuildingPassiveEffectData
	{
		public SerializedBuildingPassive Container;

		public int GenerateLightFogIndex;

		public SerializedBuildingPassiveEffectData(SerializedBuildingPassive container)
		{
			Container = container;
			GenerateLightFogIndex = 0;
		}
	}

	public static BuildingPassiveEffect PassiveEffectFromDefinition(BuildingPassiveEffectDefinition effectDefinition, PassivesModule passivesModule, SerializedBuildingPassiveEffectData serializedData = null)
	{
		if (!(effectDefinition is GenerateLightFogDefinition generateLightFogDefinition))
		{
			if (!(effectDefinition is GainResourcesDefinition gainResourcesDefinition))
			{
				if (!(effectDefinition is FillEffectGaugeDefinition fillEffectGaugeDefinition))
				{
					if (!(effectDefinition is GenerateNewItemsRosterDefinition buildingPassiveDefinition))
					{
						if (!(effectDefinition is ImproveSpawnWaveInfoDefinition buildingDefinition))
						{
							if (!(effectDefinition is IncreaseWorkersDefinition increaseWorkersDefinition))
							{
								if (!(effectDefinition is TransformBuildingDefinition transformBuildingDefinition))
								{
									if (!(effectDefinition is DestroyBuildingDefinition destroyBuildingDefinition))
									{
										if (!(effectDefinition is UpdateShopLevelDefinition updateShopLevelDefinition))
										{
											if (!(effectDefinition is UpdateBonePileLevelDefinition updateBonePileLevelDefinition))
											{
												if (effectDefinition is GenerateGuardianDefinition generateGuardianDefinition)
												{
													return new GenerateGuardianController(passivesModule, generateGuardianDefinition).BuildingPassiveEffect;
												}
												return null;
											}
											return new UpdateBonePileLevelController(passivesModule, updateBonePileLevelDefinition).BuildingPassiveEffect;
										}
										return new UpdateShopLevelController(passivesModule, updateShopLevelDefinition).BuildingPassiveEffect;
									}
									return new DestroyBuildingController(passivesModule, destroyBuildingDefinition).BuildingPassiveEffect;
								}
								return new TransformBuildingController(passivesModule, transformBuildingDefinition).BuildingPassiveEffect;
							}
							return new IncreaseWorkersController(passivesModule, increaseWorkersDefinition).BuildingPassiveEffect;
						}
						return new ImproveSpawnWaveInfoController(passivesModule, buildingDefinition).BuildingPassiveEffect;
					}
					return new GenerateNewItemsRosterController(passivesModule, buildingPassiveDefinition).BuildingPassiveEffect;
				}
				return new FillEffectGaugeController(passivesModule, fillEffectGaugeDefinition).BuildingPassiveEffect;
			}
			return new GainResourcesController(passivesModule, gainResourcesDefinition).BuildingPassiveEffect;
		}
		return (serializedData?.Container?.GenerateLightFogEffects != null && serializedData.GenerateLightFogIndex < serializedData.Container.GenerateLightFogEffects.Count) ? new GenerateLightFogController(serializedData.Container.GenerateLightFogEffects[serializedData.GenerateLightFogIndex++], passivesModule, generateLightFogDefinition).BuildingPassiveEffect : new GenerateLightFogController(passivesModule, generateLightFogDefinition).BuildingPassiveEffect;
	}
}
