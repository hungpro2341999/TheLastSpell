using TMPro;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class TileCoordinatesView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI coordinatesText;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private Vector2 offset = Vector2.zero;

	private void OnToggleValueChanged(bool value)
	{
		coordinatesText.gameObject.SetActive(value);
	}

	private void Awake()
	{
		if (toggle != null)
		{
			toggle.onValueChanged.AddListener(delegate(bool value)
			{
				OnToggleValueChanged(value);
			});
		}
	}

	private void Update()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile == null)
		{
			coordinatesText.enabled = false;
			return;
		}
		coordinatesText.enabled = true;
		coordinatesText.text = $"{TPSingleton<GameManager>.Instance.Game.Cursor.Tile.X},{TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Y}";
		Vector3 cellCenterWorldPosition = TileMapView.GetCellCenterWorldPosition(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
		coordinatesText.transform.position = cellCenterWorldPosition + (Vector3)offset;
	}
}
