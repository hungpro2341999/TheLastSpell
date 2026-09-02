using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Unit;

public class PlayableUnitGhostView : PlayableUnitView
{
	[SerializeField]
	private DataColor validColor;

	[SerializeField]
	private DataColor invalidColor;

	public void ChangeColors(bool isValid)
	{
		ChangeColors(isValid ? validColor._Color : invalidColor._Color);
	}

	public void Display(bool displayed)
	{
		if (base.gameObject.activeSelf != displayed)
		{
			if (displayed)
			{
				RefreshBodyParts();
			}
			base.gameObject.SetActive(displayed);
			if (!base.AreAnimationsInitialized)
			{
				InitAndStartAnimations(playSpawnAnim: false);
			}
		}
	}

	public void FollowMouse()
	{
		Vector3 worldPosition = TileMapView.GetWorldPosition(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
		base.transform.position = new Vector3(worldPosition.x, worldPosition.y, base.transform.position.z);
	}

	public override void PrepareForSnapshot()
	{
		base.PrepareForSnapshot();
		ChangeColors(Color.white);
		if (!base.AreAnimationsInitialized)
		{
			InitAndStartAnimations(playSpawnAnim: false);
		}
	}

	protected void ChangeColors(Color col)
	{
		for (int i = 0; i < bodyPartViews.Length; i++)
		{
			bodyPartViews[i].Tint(col);
		}
	}

	protected override void InitHud()
	{
	}
}
