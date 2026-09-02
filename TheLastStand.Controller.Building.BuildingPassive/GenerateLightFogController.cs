using System.Collections.Generic;
using System.Linq;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveEffect;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class GenerateLightFogController : BuildingPassiveEffectController
{
	public GenerateLightFog GenerateLightFog => base.BuildingPassiveEffect as GenerateLightFog;

	public GenerateLightFogController(SerializedGenerateLightFog container, PassivesModule buildingPassivesModule, GenerateLightFogDefinition generateLightFogDefinition)
	{
		base.BuildingPassiveEffect = new GenerateLightFog(container, buildingPassivesModule, generateLightFogDefinition, this);
		FogController.RegisterSupplier(GenerateLightFog);
	}

	public GenerateLightFogController(PassivesModule buildingPassivesModule, GenerateLightFogDefinition generateLightFogDefinition)
	{
		base.BuildingPassiveEffect = new GenerateLightFog(buildingPassivesModule, generateLightFogDefinition, this);
	}

	public override void Apply()
	{
		FogController.SetLightFogTilesFromDictionnary(FogController.IncrementLightFogTilesBuffer(GetTilesFromPattern()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
		if (!GenerateLightFog.GenerateLightFogDefinition.CanLightFogExistOnSelf)
		{
			FogController.SetLightFogTilesFromDictionnary(FogController.ToggleLightFogTiles(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OccupiedTiles), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
		}
	}

	public override void Unapply()
	{
		FogController.UnregisterSupplier(GenerateLightFog);
		FogController.SetLightFogTilesFromDictionnary(FogController.DecrementLightFogTilesBuffer(GetTilesFromPattern()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
	}

	private List<Tile> GetTilesFromPattern()
	{
		List<Tile> list = new List<Tile>();
		Vector2Int position = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OriginTile.Position;
		foreach (Vector2Int item in GenerateLightFog.Pattern)
		{
			Tile tile = TileMapManager.GetTile(position.x + item.x, position.y + item.y);
			if (tile != null)
			{
				list.Add(tile);
			}
		}
		if (GenerateLightFog.GenerateLightFogDefinition.CanLightFogExistOnSelf)
		{
			list.AddRange(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OccupiedTiles);
			return list.Distinct().ToList();
		}
		return list.Except(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OccupiedTiles).ToList();
	}
}
