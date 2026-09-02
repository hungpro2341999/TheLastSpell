using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using TheLastStand.View.HUD.BottomScreenPanel;
using UnityEngine;

namespace TheLastStand.View;

public class GameView : TPSingleton<GameView>
{
	[SerializeField]
	private GameObject mainCanvasGameObject;

	[SerializeField]
	private TopScreenPanel topScreenPanel;

	[SerializeField]
	private BottomScreenPanel bottomScreenPanel;

	[SerializeField]
	private GameAccelerationPanel gameAccelerationPanel;

	[SerializeField]
	private CharacterDetailsView characterDetailsView;

	[SerializeField]
	private Canvas mainHUDCanvas;

	[SerializeField]
	private DataColor positiveColor;

	[SerializeField]
	private DataColor negativeColor;

	public static CharacterDetailsView CharacterDetailsView
	{
		get
		{
			if (TPSingleton<GameView>.Instance.characterDetailsView == null)
			{
				TPSingleton<PlayableUnitManager>.Instance.LogWarning("characterDetailsView is missing in " + TPSingleton<GameView>.Instance.GetType().Name + ", getting it with FindObjectOfType!");
				TPSingleton<GameView>.Instance.characterDetailsView = Object.FindObjectOfType<CharacterDetailsView>();
			}
			return TPSingleton<GameView>.Instance.characterDetailsView;
		}
	}

	public static BottomScreenPanel BottomScreenPanel
	{
		get
		{
			if (TPSingleton<GameView>.Instance.bottomScreenPanel == null)
			{
				TPSingleton<BuildingManager>.Instance.LogWarning("bottomScreenPanel is missing in " + TPSingleton<GameView>.Instance.GetType().Name + ", getting it with FindObjectOfType!");
				TPSingleton<GameView>.Instance.bottomScreenPanel = Object.FindObjectOfType<BottomScreenPanel>();
			}
			return TPSingleton<GameView>.Instance.bottomScreenPanel;
		}
	}

	public static GameAccelerationPanel GameAccelerationPanel => TPSingleton<GameView>.Instance.gameAccelerationPanel;

	public static Color NegativeColor => TPSingleton<GameView>.Instance.negativeColor._Color;

	public static Color PositiveColor => TPSingleton<GameView>.Instance.positiveColor._Color;

	public static TopScreenPanel TopScreenPanel => TPSingleton<GameView>.Instance.topScreenPanel;

	public GameObject MainCanvasGameObject => mainCanvasGameObject;

	public void DisplayHud()
	{
		StartCoroutine(DisplayHudCoroutine());
	}

	public void HideHud()
	{
		mainHUDCanvas.enabled = false;
		topScreenPanel.TurnPanel.TurnPanelCanvas.enabled = false;
		topScreenPanel.UnitPortraitsPanel.PortraitsCanvas.enabled = false;
	}

	protected override void Awake()
	{
		base.Awake();
		mainCanvasGameObject.GetComponent<TPEnableGOsOnGameStart>()?.EnableTargets();
	}

	private IEnumerator DisplayHudCoroutine()
	{
		mainHUDCanvas.enabled = true;
		Canvas canvas = mainHUDCanvas;
		int sortingOrder = canvas.sortingOrder + 1;
		canvas.sortingOrder = sortingOrder;
		yield return SharedYields.WaitForEndOfFrame;
		Canvas canvas2 = mainHUDCanvas;
		sortingOrder = canvas2.sortingOrder - 1;
		canvas2.sortingOrder = sortingOrder;
		topScreenPanel.TurnPanel.TurnPanelCanvas.enabled = true;
		topScreenPanel.UnitPortraitsPanel.PortraitsCanvas.enabled = true;
	}
}
