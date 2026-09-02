using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class DestroyBuildingFeedback : MonoBehaviour
{
	[SerializeField]
	[Range(0.01f, 0.1f)]
	private float destroySpeed = 0.05f;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Image progressImage;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private Color startColor = Color.white;

	[SerializeField]
	private Color endColor = Color.red;

	[SerializeField]
	private Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f);

	[SerializeField]
	private Vector3 endScale = new Vector3(1f, 1f, 1f);

	private float destroyProgress;

	private TheLastStand.Model.Building.Building currentBuilding;

	private bool hasDestroyedBuildingOnTile;

	public void Toggle(bool toggle)
	{
		base.gameObject.SetActive(toggle);
		if (!toggle)
		{
			Reset(null);
		}
	}

	protected virtual void Awake()
	{
		Toggle(toggle: false);
	}

	protected virtual void Update()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
		{
			hasDestroyedBuildingOnTile = false;
		}
		if (hasDestroyedBuildingOnTile || TPSingleton<ConstructionManager>.Instance.Construction.DestroyMode == TheLastStand.Model.Building.Construction.E_DestroyMode.None)
		{
			return;
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (tile?.Building != null && (tile.Building.BlueprintModule.IsIndestructible || !tile.Building.DamageableModule.IsDead))
		{
			if (tile.Building != currentBuilding)
			{
				Reset(tile.Building);
			}
			if (!tile.Building.ConstructionModule.IsDemolishable)
			{
				return;
			}
			if (InputManager.GetButton(24))
			{
				destroyProgress = Mathf.Clamp(destroyProgress + destroySpeed, 0f, 1f);
			}
			else
			{
				Reset(tile.Building);
			}
			Color color = Color.Lerp(startColor, endColor, destroyProgress);
			progressImage.fillAmount = destroyProgress;
			progressImage.color = color;
			if (!(destroyProgress > 0f))
			{
				return;
			}
			Vector3 localScale = Vector3.Lerp(startScale, endScale, destroyProgress);
			progressText.text = $"{(int)(100f * destroyProgress)}%";
			progressText.color = color;
			progressText.transform.localScale = localScale;
			text.color = color;
			text.transform.localScale = localScale;
			progressText.enabled = true;
			if (destroyProgress >= 1f)
			{
				hasDestroyedBuildingOnTile = true;
				if (tile.Building.BuildingController.DamageableModuleController != null)
				{
					tile.Building.BuildingController.DamageableModuleController.Demolish();
				}
				else
				{
					BuildingManager.DestroyBuilding(tile, updateView: true, addDeadBuilding: false, triggerEvent: false, triggerOnDeathEvent: false);
				}
				Reset(null);
			}
		}
		else
		{
			Reset(null);
		}
	}

	private void Reset(TheLastStand.Model.Building.Building building)
	{
		destroyProgress = 0f;
		currentBuilding = building;
		if (currentBuilding != null)
		{
			if (currentBuilding.ConstructionModule.IsDemolishable)
			{
				text.text = Localizer.Format("ConstructionPanel_DestroyNoCostTip", currentBuilding.BuildingDefinition.Name);
				text.enabled = true;
				progressText.enabled = true;
				progressImage.enabled = true;
			}
			else
			{
				text.text = Localizer.Format("ConstructionPanel_CantDestroy", currentBuilding.BuildingDefinition.Name);
				text.enabled = true;
				progressText.enabled = false;
				progressImage.enabled = false;
			}
		}
		else
		{
			text.enabled = false;
			progressText.enabled = false;
			progressImage.enabled = false;
		}
		text.color = startColor;
		text.transform.localScale = startScale;
	}
}
