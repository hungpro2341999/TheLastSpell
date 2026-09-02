using System.Collections.Generic;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;

namespace TheLastStand.Controller.Unit.Enemy.Affix;

public class EnemyMistyAffixController : EnemyAffixController
{
	public EnemyMistyAffix EnemyMistyAffix => base.EnemyAffix as EnemyMistyAffix;

	public List<Tile> LightFogTiles { get; private set; } = new List<Tile>();

	public EnemyMistyAffixController(EnemyAffixDefinition enemyAffixDefinition, EnemyUnit enemyUnit)
	{
		base.EnemyAffix = new EnemyMistyAffix(this, enemyAffixDefinition, enemyUnit);
		FogController.RegisterSupplier(EnemyMistyAffix);
	}

	public override void Trigger(E_EffectTime effectTime, object data = null)
	{
		switch (effectTime)
		{
		case E_EffectTime.OnDeath:
			FogController.UnregisterSupplier(EnemyMistyAffix);
			DecrementLightFogInRange();
			break;
		case E_EffectTime.OnCreationAfterViewInitialized:
			IncrementLightFogInRange();
			break;
		}
	}

	public void TriggerLightFogDuringMovement(Tile previousTile, Tile currentTile)
	{
		EnemyMistyAffix.LightFogSupplierMoveDatas.CurrentTile = currentTile;
		List<Tile> lightFogTiles = EnemyMistyAffix.EnemyMistyAffixController.LightFogTiles;
		List<Tile> tilesInRange = EnemyMistyAffix.EnemyMistyAffixController.GetTilesInRange(currentTile);
		List<Tile> list = new List<Tile>();
		List<Tile> list2 = new List<Tile>();
		for (int i = 0; i < lightFogTiles.Count; i++)
		{
			if (!tilesInRange.Contains(lightFogTiles[i]))
			{
				list2.Add(lightFogTiles[i]);
			}
		}
		for (int j = 0; j < tilesInRange.Count; j++)
		{
			if (!lightFogTiles.Contains(tilesInRange[j]))
			{
				list.Add(tilesInRange[j]);
			}
		}
		Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode = FogController.IncrementLightFogTilesBuffer(list);
		Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode2 = FogController.DecrementLightFogTilesBuffer(list2);
		Dictionary<Fog.LightFogTileInfo.E_LightFogMode, List<Tile>> tilesToUpdateByLightFogMode3 = FogController.ToggleLightFogTiles(new List<Tile> { previousTile, currentTile });
		FogController.SetLightFogTilesFromDictionnary(tilesToUpdateByLightFogMode, FogManager.MovingLightFogFadeInEaseAndDuration, FogManager.MovingLightFogFadeOutEaseAndDuration, FogManager.MovingLightFogDisappearEaseAndDuration, instant: false, independently: true);
		FogController.SetLightFogTilesFromDictionnary(tilesToUpdateByLightFogMode2, FogManager.MovingLightFogFadeInEaseAndDuration, FogManager.MovingLightFogFadeOutEaseAndDuration, FogManager.MovingLightFogDisappearEaseAndDuration, instant: false, independently: true);
		FogController.SetLightFogTilesFromDictionnary(tilesToUpdateByLightFogMode3, FogManager.MovingLightFogFadeInEaseAndDuration, FogManager.MovingLightFogFadeOutEaseAndDuration, FogManager.MovingLightFogDisappearEaseAndDuration, instant: false, independently: true);
	}

	public List<Tile> GetTilesInRange(Tile tile = null)
	{
		LightFogTiles = base.EnemyAffix.EnemyUnit.OccupiedTiles.GetTilesInRange(EnemyMistyAffix.Range);
		return LightFogTiles;
	}

	private void IncrementLightFogInRange()
	{
		FogController.SetLightFogTilesFromDictionnary(FogController.IncrementLightFogTilesBuffer(GetTilesInRange()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
	}

	private void DecrementLightFogInRange()
	{
		FogController.SetLightFogTilesFromDictionnary(FogController.DecrementLightFogTilesBuffer(LightFogTiles), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration);
	}
}
