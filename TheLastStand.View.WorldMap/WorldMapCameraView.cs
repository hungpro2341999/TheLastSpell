using TPLib;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Camera;
using TheLastStand.View.WorldMap.ItemRestriction;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class WorldMapCameraView : ACameraView
{
	public static void OnStateChange()
	{
		switch (TPSingleton<WorldMapStateManager>.Instance.CurrentState)
		{
		case WorldMapStateManager.WorldMapState.EXPLORATION:
			ACameraView.Zoom(zoomIn: false);
			CheckIfCanMoveCameraInExplorationState();
			break;
		case WorldMapStateManager.WorldMapState.FOCUSED:
		{
			Vector3 cameraPositionToFocusZoomedCity = GetCameraPositionToFocusZoomedCity(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityView.TargetPos.transform.position);
			ACameraView.Zoom(zoomIn: true);
			ACameraView.MoveTo(cameraPositionToFocusZoomedCity);
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		}
		case WorldMapStateManager.WorldMapState.APOCALYPSE_MAX_LEVEL_FOCUS:
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		case WorldMapStateManager.WorldMapState.GLYPHSELECTION:
		case WorldMapStateManager.WorldMapState.APOCALYPSE_SELECTION:
			break;
		}
	}

	public static void CheckIfCanMoveCameraInExplorationState()
	{
		if (TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.EXPLORATION || TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.DEFAULT)
		{
			if (TPSingleton<WeaponRestrictionsPanel>.Instance.Displayed)
			{
				ACameraView.AllowUserPan = false;
				ACameraView.AllowUserZoom = false;
			}
			else
			{
				ACameraView.AllowUserPan = true;
				ACameraView.AllowUserZoom = true;
			}
		}
	}

	private static Vector3 GetCameraPositionToFocusZoomedCity(Vector3 cityWorldPosition)
	{
		WorldMapCameraView obj = TPSingleton<ACameraView>.Instance as WorldMapCameraView;
		CanvasScaler canvasScaler = TPSingleton<WorldMapUIManager>.Instance.CanvasScaler;
		float num = TPSingleton<GameConfigurationsView>.Instance.BoxWithoutDecorationsTransform.sizeDelta.x * canvasScaler.scaleFactor;
		int width = Screen.width;
		float num2 = (float)width - num;
		float num3 = obj.pixelPerfectCamOriginalPpu;
		float num4 = obj.ComputeTargetPixelsPerUnit(zoomedIn: true);
		float num5 = ACameraView.MainCam.orthographicSize;
		if (!ACameraView.IsZoomedIn)
		{
			num5 /= num4 / num3;
		}
		float num6 = num5 * 2f * ACameraView.MainCam.aspect;
		float num7 = (num + num2 / 2f) / (float)width;
		cityWorldPosition.x -= num6 * (num7 - 0.5f);
		return cityWorldPosition;
	}
}
