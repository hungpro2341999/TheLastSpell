using TheLastStand.Definition.DLC;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuDLCDisplay : MonoBehaviour
{
	[SerializeField]
	private Image dlcIcon;

	public DLCDefinition DLCDefinition { get; private set; }

	public void SetContent(DLCDefinition dlcDefinition)
	{
		DLCDefinition = dlcDefinition;
	}

	public void Refresh()
	{
		if (!(DLCDefinition == null) && dlcIcon != null)
		{
			dlcIcon.sprite = DLCDefinition.IconSprite;
		}
	}
}
