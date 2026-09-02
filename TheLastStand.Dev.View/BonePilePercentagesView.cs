using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Dev.View;

public class BonePilePercentagesView : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Vector2 offset = Vector2.zero;

	public void Toggle(bool state)
	{
		canvas.enabled = state;
	}

	private void Awake()
	{
		Toggle(state: false);
	}

	private void Update()
	{
		if (!canvas.enabled || !TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
		{
			return;
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (tile != null)
		{
			string text = $"{tile.Id} (city distance={tile.DistanceToCity})\n";
			if (TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(tile, out var value))
			{
				foreach (KeyValuePair<string, int> item in value)
				{
					text += $"{item.Key}: {item.Value}%\n";
				}
			}
			else
			{
				text += "No percentage";
			}
			this.text.text = text;
			this.text.enabled = true;
			Vector3 cellCenterWorldPosition = TileMapView.GetCellCenterWorldPosition(tile);
			this.text.transform.position = cellCenterWorldPosition + (Vector3)offset;
		}
		else
		{
			this.text.enabled = false;
		}
	}
}
