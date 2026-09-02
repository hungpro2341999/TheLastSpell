using System.Linq;
using TMPro;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.View.LevelEditor;

public class GroundCounterView : MonoBehaviour
{
	[SerializeField]
	private string groundId = string.Empty;

	[SerializeField]
	private TextMeshProUGUI groundIdText;

	[SerializeField]
	private TextMeshProUGUI countText;

	public void Refresh()
	{
		if (base.gameObject.activeInHierarchy)
		{
			groundIdText.text = groundId;
			countText.text = ((groundId == "Total") ? TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Length.ToString() : TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Count((Tile o) => o.GroundDefinition.Id == groundId).ToString());
		}
	}

	private void OnEnable()
	{
		Refresh();
	}
}
