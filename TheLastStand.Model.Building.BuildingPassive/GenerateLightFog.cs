using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Database.Fog;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveEffect;
using UnityEngine;

namespace TheLastStand.Model.Building.BuildingPassive;

public class GenerateLightFog : BuildingPassiveEffect, ILightFogSupplier
{
	private string patternId;

	public GenerateLightFogDefinition GenerateLightFogDefinition => base.BuildingPassiveEffectDefinition as GenerateLightFogDefinition;

	public List<Vector2Int> Pattern { get; private set; }

	public bool CanLightFogExistOnSelf => GenerateLightFogDefinition.CanLightFogExistOnSelf;

	public bool IsLightFogSupplierMoving { get; set; }

	public LightFogSupplierMoveDatas LightFogSupplierMoveDatas { get; set; }

	public bool CanLightFogSupplierMove => false;

	public GenerateLightFog(SerializedGenerateLightFog container, PassivesModule buildingPassivesModule, GenerateLightFogDefinition buildingPassiveDefinition, GenerateLightFogController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
		Deserialize(container);
		if (!FogDatabase.LightFogDefinition.Patterns.ContainsKey(patternId))
		{
			CLoggerManager.Log("Deserialized a key (" + patternId + ") that wasn't found in the database. Taking a random one instead.");
			patternId = FogDatabase.LightFogDefinition.Patterns.Keys.ElementAt(RandomManager.GetRandomRange(TPSingleton<FogManager>.Instance, 0, FogDatabase.LightFogDefinition.Patterns.Keys.Count));
		}
		Pattern = FogDatabase.LightFogDefinition.Patterns[patternId];
	}

	public GenerateLightFog(PassivesModule buildingPassivesModule, GenerateLightFogDefinition buildingPassiveDefinition, GenerateLightFogController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
		patternId = FogDatabase.LightFogDefinition.Patterns.Keys.ElementAt(RandomManager.GetRandomRange(TPSingleton<FogManager>.Instance, 0, FogDatabase.LightFogDefinition.Patterns.Keys.Count));
		Pattern = FogDatabase.LightFogDefinition.Patterns[patternId];
	}

	public void Deserialize(SerializedGenerateLightFog container)
	{
		patternId = container.PatternId;
	}

	public SerializedGenerateLightFog Serialize()
	{
		return new SerializedGenerateLightFog
		{
			PatternId = patternId
		};
	}
}
