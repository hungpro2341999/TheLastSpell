using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.UnitManagement.UI;

public class MovePathCounterHUD : TPSingleton<MovePathCounterHUD>
{
	[SerializeField]
	private Transform movePathCountContainer;

	[SerializeField]
	private TextMeshProUGUI movePathCountText;

	[SerializeField]
	private DataColor movePathCountValidColor;

	[SerializeField]
	private DataColor movePathCountInvalidColor;

	[SerializeField]
	private LocalizedFont localizedFont;

	public MovePath MovePath { get; set; }

	public TextMeshProUGUI MovePathCountText => movePathCountText;

	public void ChangeCounterColor(Color newColor)
	{
		movePathCountText.color = newColor;
	}

	public void DisplayMovePathCount(bool isDisplayed = true)
	{
		if (!(movePathCountContainer == null))
		{
			if (!isDisplayed || MovePath.Path.Count == 0)
			{
				movePathCountContainer.gameObject.SetActive(value: false);
				return;
			}
			Tile tile = MovePath.Path[MovePath.Path.Count - 1];
			movePathCountContainer.position = TileMapView.GetWorldPosition(tile);
			movePathCountText.text = (MovePath.Path.Count - 1).ToString();
			movePathCountContainer.gameObject.SetActive(value: true);
			localizedFont?.RefreshFont();
		}
	}

	public void SetMovePathCountValidation(bool isValid)
	{
		ChangeCounterColor(isValid ? movePathCountValidColor._Color : movePathCountInvalidColor._Color);
	}

	protected override void Awake()
	{
		base.Awake();
		if (movePathCountContainer == null && movePathCountText != null)
		{
			movePathCountContainer = movePathCountText.transform;
		}
	}
}
