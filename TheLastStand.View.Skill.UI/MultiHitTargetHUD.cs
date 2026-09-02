using Sirenix.OdinInspector;
using TMPro;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class MultiHitTargetHUD : SerializedMonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI counterText;

	private int counter;

	public int Counter
	{
		get
		{
			return counter;
		}
		set
		{
			if (counter != value)
			{
				counter = value;
				RefreshCounter();
			}
		}
	}

	public Tile TargetTile { get; set; }

	private void OnDisable()
	{
		counter = 0;
	}

	private void RefreshCounter()
	{
		counterText.text = $"{counter}";
	}
}
