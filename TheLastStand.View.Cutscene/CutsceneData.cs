using TPLib;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public struct CutsceneData
{
	public WorldMapCity City;

	public Tile Tile;

	public TheLastStand.Model.Unit.Unit Unit;

	public Vector3? Position;

	public CutsceneData(WorldMapCity city = null, Tile tile = null, TheLastStand.Model.Unit.Unit unit = null, Vector3? position = null)
	{
		City = city ?? TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		Tile = tile;
		Unit = unit;
		Position = position ?? ((unit != null) ? new Vector3?(unit.OriginTile.TileView.transform.position) : tile?.TileView.transform.position);
	}
}
