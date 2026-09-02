using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Manager;

public sealed class SectorManager : Manager<SectorManager>
{
	public static class Constants
	{
		public static class Assets
		{
			public const string SectorContainerPrefabPathFormat = "Prefab/Sectors/{0}/{0}_Sectors";
		}
	}

	[SerializeField]
	[Range(0f, 10f)]
	private int targetFocusCameraWeight = 2;

	[SerializeField]
	[Range(0f, 10f)]
	private int casterFocusCameraWeight = 1;

	private bool initialized;

	public bool TestCameraSectorOnClick;

	public bool WillMoveCameraNextFrame;

	public SectorContainer SectorContainer { get; private set; }

	public List<CameraAreaOfInterest> Sectors => SectorContainer?.Sectors;

	public int SectorsCount => Sectors?.Count ?? 0;

	public int TargetFocusCameraWeight => targetFocusCameraWeight;

	public int CasterFocusCameraWeight => casterFocusCameraWeight;

	public int GetSectorIndexForTile(Tile tile)
	{
		if (tile == null || Sectors == null)
		{
			LogError("Error while trying to get sector for tile : " + ((tile == null) ? "Tile is" : "Sectors are") + " null.");
			return -1;
		}
		Vector3 tileWorldPos = TileMapView.GetTileCenter(tile);
		IEnumerable<CameraAreaOfInterest> source = from x in Sectors
			where x.AreaCollider.OverlapPoint(tileWorldPos)
			orderby x.AreaWeight descending
			select x;
		CameraAreaOfInterest cameraAreaOfInterest = ((source.Count() == 0) ? Sectors.OrderBy((CameraAreaOfInterest x) => (x.transform.position - tileWorldPos).magnitude - x.AreaWeight).First() : source.First());
		Log(string.Format("Asked for sector index for tile {0}, returning {1}{2}", tile.Position, source.Any() ? string.Empty : "default: ", cameraAreaOfInterest.name));
		return Sectors.IndexOf(cameraAreaOfInterest);
	}

	public void Init()
	{
		if (!initialized)
		{
			string text = ((ApplicationManager.Application.State.GetName() == "LevelEditor") ? LevelEditorManager.CityToLoadId : TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.SectorContainerPrefabId);
			string text2 = string.Format("Prefab/Sectors/{0}/{0}_Sectors", text);
			SectorContainer sectorContainer = ResourcePooler.LoadOnce<SectorContainer>(text2);
			if (sectorContainer != null)
			{
				Log("Prefab has been found for city Id " + text + " at " + text2 + ".", CLogLevel.MAJOR);
				SectorContainer = Object.Instantiate(sectorContainer, base.transform);
			}
			else
			{
				LogError("No Sector prefab has been found for city Id " + text + ".", CLogLevel.MAJOR);
			}
			initialized = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void Update()
	{
		DebugUpdate();
	}

	[DevConsoleCommand("TestCameraSectorOnClick")]
	[ContextMenu("TestCameraSectorOnClick")]
	public static void DebugTestCameraSectorOnClick(bool state = true)
	{
		TPSingleton<SectorManager>.Instance.TestCameraSectorOnClick = state;
	}

	private void DebugUpdate()
	{
		if (InputManager.GetButton(24) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
		{
			if (!TPSingleton<SectorManager>.Instance.TestCameraSectorOnClick)
			{
				return;
			}
			if (!WillMoveCameraNextFrame)
			{
				WillMoveCameraNextFrame = true;
				return;
			}
			Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
			if (tile != null)
			{
				int sectorIndexForTile = TPSingleton<SectorManager>.Instance.GetSectorIndexForTile(tile);
				if (TPSingleton<SectorManager>.Instance.SectorsCount > sectorIndexForTile)
				{
					CameraAreaOfInterest cameraAreaOfInterest = TPSingleton<SectorManager>.Instance.Sectors[sectorIndexForTile];
					Log($"TestCameraSectorOnClick was triggered for tile {tile.Position}, moving to sector {cameraAreaOfInterest.name} at position {cameraAreaOfInterest.transform.position}.", CLogLevel.NORMAL, forcePrintInUnity: true);
					ACameraView.MoveTo(cameraAreaOfInterest.transform.position, CameraView.AnimationMoveSpeed);
				}
			}
			WillMoveCameraNextFrame = false;
		}
		else if (WillMoveCameraNextFrame)
		{
			WillMoveCameraNextFrame = false;
		}
	}
}
