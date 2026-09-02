using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class ProductionPanel : MonoBehaviour
{
	[SerializeField]
	private BuildingHUD buildingHUD;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	[Tooltip("Delay before hiding (if needed), after being animated")]
	private float hideAfterAnimDelay = 0.6f;

	[SerializeField]
	private ProductionPanelGauge unitsGauge;

	[SerializeField]
	[Range(0.05f, 1f)]
	private float delayBetweenGaugeUnits = 0.2f;

	[SerializeField]
	private Image productionRewardIconImage;

	private int buildingGaugeValueToGive;

	private Coroutine currentRefreshCoroutine;

	private Queue<int> refreshProductionCoroutinesQueue = new Queue<int>();

	public ProductionPanelGauge UnitsGauge => unitsGauge;

	private Sprite ProductionRewardIconSprite => buildingHUD.Building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetProductionRewardIconSpriteBig();

	private bool ShouldBeDisplayed
	{
		get
		{
			if (buildingHUD.Building.ProductionModule?.BuildingGaugeEffect != null && TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver)
			{
				return TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying;
			}
			return false;
		}
	}

	public void DisplayIfNeeded()
	{
		canvas.enabled = ShouldBeDisplayed;
	}

	public void AnimateUnitsIncrement(bool tween = false, int unitsToAdd = 0)
	{
		canvas.enabled = !buildingHUD.Building.OriginTile.HasFog;
		if (tween)
		{
			refreshProductionCoroutinesQueue.Enqueue(unitsToAdd);
			if (currentRefreshCoroutine == null)
			{
				currentRefreshCoroutine = StartCoroutine(RefreshProductionCoroutine());
			}
			return;
		}
		if (currentRefreshCoroutine != null)
		{
			StopCoroutine(currentRefreshCoroutine);
			currentRefreshCoroutine = null;
			refreshProductionCoroutinesQueue.Clear();
		}
		if (buildingHUD.Building.ProductionModule?.BuildingGaugeEffect != null)
		{
			if (unitsGauge != null)
			{
				unitsGauge.SetUnits(buildingHUD.Building.ProductionModule.BuildingGaugeEffect.Units, tween: false);
			}
			if (TPSingleton<BuildingManager>.Instance.WaitBuildingGauges.Contains(buildingHUD.Building))
			{
				TPSingleton<BuildingManager>.Instance.WaitBuildingGauges.Remove(buildingHUD.Building);
			}
		}
		DisplayIfNeeded();
	}

	public void Init()
	{
		productionRewardIconImage.sprite = ((buildingHUD.Building is MagicCircle) ? null : ProductionRewardIconSprite);
	}

	private void Awake()
	{
		canvas.worldCamera = ACameraView.MainCam;
	}

	private IEnumerator RefreshProductionCoroutine(int unitsAdded = -1)
	{
		yield return SharedYields.WaitForSeconds(0.2f);
		if (unitsAdded == -1)
		{
			unitsAdded = refreshProductionCoroutinesQueue.Dequeue();
		}
		buildingGaugeValueToGive += unitsAdded;
		if (buildingHUD.Building.ProductionModule?.BuildingGaugeEffect != null && unitsGauge != null)
		{
			while (buildingGaugeValueToGive > 0)
			{
				buildingGaugeValueToGive--;
				unitsGauge.AddUnits(1);
				if (unitsGauge.IsFull)
				{
					yield return SharedYields.WaitForSeconds(delayBetweenGaugeUnits);
					EffectManager.DisplayEffects();
					unitsGauge.Clear();
				}
				yield return SharedYields.WaitForSeconds(delayBetweenGaugeUnits);
			}
		}
		if (refreshProductionCoroutinesQueue.Count > 0)
		{
			currentRefreshCoroutine = StartCoroutine(RefreshProductionCoroutine(refreshProductionCoroutinesQueue.Dequeue()));
		}
		else
		{
			currentRefreshCoroutine = null;
			yield return SharedYields.WaitForSeconds(hideAfterAnimDelay);
			if (TPSingleton<BuildingManager>.Instance.WaitBuildingGauges.Contains(buildingHUD.Building) && refreshProductionCoroutinesQueue.Count == 0)
			{
				TPSingleton<BuildingManager>.Instance.WaitBuildingGauges.Remove(buildingHUD.Building);
			}
		}
		DisplayIfNeeded();
	}
}
