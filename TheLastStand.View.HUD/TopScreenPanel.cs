using TheLastStand.View.HUD.UnitPortraitPanel;
using UnityEngine;

namespace TheLastStand.View.HUD;

public class TopScreenPanel : MonoBehaviour
{
	[SerializeField]
	private TurnPanel turnPanel;

	[SerializeField]
	private UnitPortraitsPanel unitPortraitsPanel;

	public TurnPanel TurnPanel => turnPanel;

	public UnitPortraitsPanel UnitPortraitsPanel => unitPortraitsPanel;

	public void Display(bool show)
	{
		TurnPanel.Display(show);
		UnitPortraitsPanel.Display(show);
	}
}
